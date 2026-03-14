using LibraryMS.Core.Application.Dtos.Auth;
using LibraryMS.Core.Application.Dtos.Email;
using LibraryMS.Core.Application.Exceptions;
using LibraryMS.Core.Application.Interfaces;
using LibraryMS.Core.Domain.Common.Enum;
using LibraryMS.Core.Domain.Entities;
using LibraryMS.Core.Domain.Interfaces.Repositories;
using LibraryMS.Core.Domain.Settings;
using LibraryMS.Infrastructure.Identity.Contexts;
using LibraryMS.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LibraryMS.Infrastructure.Identity.Services
{
    public class AuthService : IAuthService
    {
        private readonly IdentityContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailService _emailService;
        private readonly JwtSettings _jwtSettings;
        private readonly IValidationService _validationService;
        private readonly IAccountRequestRepository _accountRequestRepository;
        private readonly IEmailTemplateService _emailTemplateService;

        public AuthService(
            IdentityContext context,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IOptions<JwtSettings> jwtSettings,
            IAccountRequestRepository accountRequestRepository,
            IValidationService validationService,
            IEmailTemplateService emailTemplateService,
            IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtSettings = jwtSettings.Value;
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
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

            if (user.Status == UserStatus.Blocked)
            {
                throw ApiException.Forbidden($"Account for user {user.Name + " " + user.LastName} is blocked.");
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName ?? "", loginDto.Password, false, true);

            if (!result.Succeeded)
            {
                throw ApiException.Unauthorized("Invalid email or password.");
            }

            JwtSecurityToken jwtSecurityToken = await GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            var existing = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.UserId == user.Id);

            if (existing != null)
            {
                // UPDATE existing refresh token 
                existing.Token = newRefreshToken;
                existing.Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationTime);
            }
            else
            {
                RefreshToken refreshToken = new()
                {
                    Id = Guid.NewGuid(),
                    Token = newRefreshToken,
                    Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationTime),
                    UserId = user.Id
                };

                _context.RefreshTokens.Add(refreshToken);
            }

            await _context.SaveChangesAsync();



            var roleString = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

            if (!Enum.TryParse<Roles>(roleString, out var role))
                throw new Exception($"Invalid role '{roleString}'");

            LoginResponseDto response = new()
            {
                User = new AuthUserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    LastName = user.LastName,
                    Email = user.Email ?? "",
                    UniversityId = user.UniversityId,
                    ProfileImageUrl = user.ProfileImageUrl ?? "",
                    Role = role,
                    Status = user.Status

                },
                AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                RefreshToken = newRefreshToken,
            };

            return response;
        }

        public async Task<LoginRefreshTokenResponseDto> LoginUserWithRefreshTokenAsync(string refreshTokenRequest)
        {
            RefreshToken? refreshToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshTokenRequest);

            if (refreshToken is null || refreshToken.Expires < DateTime.UtcNow)
            {
                throw ApiException.Unauthorized("Invalid or expired refresh token.");
            }

            string accesstoken = new JwtSecurityTokenHandler().WriteToken(await GenerateJwtToken(refreshToken.User!));

            refreshToken.Token = GenerateRefreshToken();
            refreshToken.Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationTime);

            await _context.SaveChangesAsync();

            return new LoginRefreshTokenResponseDto
            {
                AccessToken = accesstoken,
                RefreshToken = refreshToken.Token
            };
        }

        public async Task<bool> RevokeRefreshTokenAsync(string userId)
        {
            await _context.RefreshTokens
                .Where(rt => rt.UserId == userId)
                .ExecuteDeleteAsync();

            return true;
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

            var body = _emailTemplateService.Render("AccountCreated.html", new()
                    {
                        { "Name", newUser.Name },
                        { "UniversityID", newUser.UniversityId }
                    });


            // Send confirmation email
            await _emailService.SendAsync(new EmailRequestDto
            {
                To = newUser.Email,
                Subject = "Account Created - Pending Approval",
                HtmlBody = body
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
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationTime),
                signingCredentials: signingCredentials
            );

            return jwtSecurityToken;
        }
        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
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
