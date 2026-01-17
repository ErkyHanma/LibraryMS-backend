using LibraryMS.Core.Application.Dtos.Auth;
using LibraryMS.Core.Application.Dtos.Email;
using LibraryMS.Core.Application.Exceptions;
using LibraryMS.Core.Application.Interfaces;
using LibraryMS.Core.Domain.Common.Enum;
using LibraryMS.Core.Domain.Entities;
using LibraryMS.Core.Domain.Interfaces.Repositories;
using LibraryMS.Core.Domain.Settings;
using LibraryMS.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LibraryMS.Infrastructure.Identity.Services
{
    public class AuthService : IAuthService
    {

        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailService _emailService;
        private readonly JwtSettings _jwtSettings;
        private readonly IValidationService _validationService;
        private readonly IAccountRequestRepository _accountRequestRepository;


        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IOptions<JwtSettings> jwtSettings,
            IAccountRequestRepository accountRequestRepository,
            IValidationService validationService,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtSettings = jwtSettings.Value;
            _emailService = emailService;
            _validationService = validationService;
            _accountRequestRepository = accountRequestRepository;
        }



        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {

            // Validate input
            await _validationService.ValidateAsync(loginDto);

            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null)
            {
                throw ApiException.Conflict($"User with email {loginDto.Email} is not registered.");
            }

            if (user.Status == UserStatus.Pending)
            {
                throw ApiException.Forbidden($"Account status is {user.Status}. Please contact support."); ;
            }

            if (user.Status == UserStatus.Blocked)
            {
                throw ApiException.Forbidden($"Account for user {user.Name + " " + user.LastName} is blocked. Please try again later.");
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName ?? "", loginDto.Password, false, true);

            if (!result.Succeeded)
            {
                throw ApiException.Unauthorized("Invalid email or password.");
            }


            JwtSecurityToken jwtSecurityToken = await GenerateJwtToken(user);
            var roleString = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

            if (!Enum.TryParse<Roles>(roleString, out var role))
                throw new Exception($"Invalid role '{roleString}'");

            LoginResponseDto response = new()
            {
                User = new AuthUserDto
                {
                    Id = user.Id ?? "",
                    Name = user.Name,
                    LastName = user.LastName,
                    Email = user.Email ?? "",
                    UniversityId = user.UniversityId,
                    ProfileImageUrl = user.ProfileImageUrl ?? "",
                    Role = role
                },
                AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken)
            };

            return response;
        }

        public async Task<SignUpResponseDto> SignUpAsync(SignUpDto dto)
        {
            // Validate input
            await _validationService.ValidateAsync(dto);

            // Validate email availability
            var existingUserByEmail = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUserByEmail != null)
            {
                throw ApiException.Conflict($"An account with the email '{dto.Email}' already exists.");
            }

            // Validate university ID availability
            var existingUserByUniversityId = await _userManager.Users
                .FirstOrDefaultAsync(u => u.UniversityId == dto.UniversityId);
            if (existingUserByUniversityId != null)
            {
                throw ApiException.Conflict(
                    $"The university ID '{dto.UniversityId}' is already registered. " +
                    "If this is your ID, please contact support or try logging in."
                );
            }

            var newUser = new User
            {
                Name = dto.Name,
                LastName = dto.LastName,
                Email = dto.Email,
                UserName = dto.Email,
                Status = UserStatus.Pending,
                UniversityId = dto.UniversityId,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(newUser, dto.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                throw ApiException.BadRequest($"Failed to create account: {errors}");
            }

            var roleResult = await _userManager.AddToRoleAsync(newUser, Roles.User.ToString());
            if (!roleResult.Succeeded)
            {
                throw ApiException.InternalServerError("Account created but failed to assign user role. Please contact support.");
            }

            // Create account request for admin approval
            var accountRequest = new AccountRequest
            {
                UserId = newUser.Id,
                Status = AccountRequestStatus.Pending,
            };

            var createdRequest = await _accountRequestRepository.AddAsync(accountRequest);
            if (createdRequest == null)
            {
                throw ApiException.InternalServerError(
                    "Your account was created but the approval request failed. Please contact support."
                );
            }

            // Send confirmation email
            await _emailService.SendAsync(new EmailRequestDto
            {
                To = newUser.Email,
                Subject = "Account Created - Pending Approval",
                HtmlBody = $@"
                    <h2>Welcome to the LibraryMS, {newUser.Name}!</h2>
                    <p>Your account has been created successfully and is pending administrator approval.</p>
                    <p>You will receive another email once your account has been approved.</p>
                    <p><strong>University ID:</strong> {newUser.UniversityId}</p>
                    <p>If you have any questions, please contact the library administration.</p>
                "
            });

            var userRoles = await _userManager.GetRolesAsync(newUser);

            return new SignUpResponseDto
            {
                Id = newUser.Id,
                Name = newUser.Name,
                LastName = newUser.LastName,
                Email = newUser.Email,
                UniversityId = newUser.UniversityId,
                Roles = userRoles.ToList(),
            };

        }

        public async Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            ForgotPasswordResponseDto response = new() { HasError = false, Errors = [] };

            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                throw ApiException.Unauthorized($"There is no account registered with this email {request.Email}");
            }

            // Disable user
            user.EmailConfirmed = false;
            await _userManager.UpdateAsync(user);


            string? resetToken = await GetResetPasswordToken(user);

            await _emailService.SendAsync(new EmailRequestDto()
            {
                To = user.Email,
                HtmlBody = $"Please reset your password account use this token {resetToken}",
                Subject = "Reset password"
            });

            return response;
        }

        public async Task<ForgotPasswordResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            ForgotPasswordResponseDto response = new() { HasError = false, Errors = [] };

            var user = await _userManager.FindByIdAsync(request.Id);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"There is no account registered with this user");
                return response;
            }

            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
            var result = await _userManager.ResetPasswordAsync(user, token, request.Password);
            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Errors.AddRange(result.Errors.Select(s => s.Description).ToList());
                return response;
            }

            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            return response;
        }

        public async Task<string> ConfirmAccountAsync(string userId, string token)
        {

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return "There's no account registered with this user";
            }

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (result.Succeeded)
            {
                return $"Account confirmed for {user.Email}. You can now use the app";
            }
            else
            {
                return $"An error occurred while confirming this email {user.Email}";
            }

        }



        #region "Private methods"
        private async Task<JwtSecurityToken> GenerateJwtToken(User user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            var rolesClaims = new List<Claim>();
            foreach (var role in roles)
            {
                rolesClaims.Add(new Claim("roles", role));
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim("uid",user.Id ?? "")
            }.Union(userClaims).Union(rolesClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                signingCredentials: signingCredentials
            );

            return jwtSecurityToken;
        }
        protected async Task<string?> GetVerificationEmailToken(User user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            return token;
        }
        protected async Task<string?> GetResetPasswordToken(User user)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            return token;
        }

        #endregion
    }
}
