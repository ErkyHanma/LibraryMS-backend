using ArtemisBanking.Core.Application.Dtos.Auth;
using LibraryMS_API.Core.Application.Dtos.Auth;
using LibraryMS_API.Core.Application.Dtos.Email;
using LibraryMS_API.Core.Application.Exceptions;
using LibraryMS_API.Core.Application.Interfaces;
using LibraryMS_API.Core.Domain.Common.Enum;
using LibraryMS_API.Core.Domain.Settings;
using LibraryMS_API.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LibraryMS_API.Infrastructure.Identity.Services
{
    public class AuthService : IAuthService
    {

        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailService _emailService;
        private readonly JwtSettings _jwtSettings;


        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IOptions<JwtSettings> jwtSettings,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtSettings = jwtSettings.Value;
            _emailService = emailService;
        }



        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {

            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null)
            {
                throw ApiException.Conflict($"Email {loginDto.Email} is already taken.");
            }

            if (user.Status == UserStatus.Pending)
            {
                throw ApiException.Forbidden($"Account status is {user.Status}. Please contact support."); ;
            }

            if (user.Status == UserStatus.Blocked)
            {
                throw ApiException.Forbidden($"Account for user {user.FullName} is blocked. Please try again later.");
            }

            var result = await _signInManager.PasswordSignInAsync(user.Email ?? "", loginDto.Password, false, true);

            if (!result.Succeeded)
            {
                throw ApiException.Unauthorized("Invalid email or password.");
            }


            JwtSecurityToken jwtSecurityToken = await GenerateJwtToken(user);

            LoginResponseDto response = new()
            {
                FullName = user.FullName,
                AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken)
            };

            return response;
        }

        public async Task<SignUpResponseDto> SignUpAsync(SignUpDto dto)
        {
            // Todo: Add unique UniversityId

            var userWithSameEmail = await _userManager.FindByEmailAsync(dto.Email);
            if (userWithSameEmail != null)
            {
                throw ApiException.Conflict($"Email {dto.Email} is already taken.");
            }


            User user = new()
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Status = UserStatus.Pending,
                UniversityId = dto.UniversityId,
                EmailConfirmed = true,
            };

            var result = await _userManager.CreateAsync(user, dto.Password);


            if (!result.Succeeded)
            {
                throw ApiException.Conflict($"Email {dto.Email} is already taken.");
            }

            await _userManager.AddToRoleAsync(user, Roles.User.ToString());

            var rolesList = await _userManager.GetRolesAsync(user);

            SignUpResponseDto response = new()
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                UniversityId = user.UniversityId,
                Roles = rolesList.ToList()

            };

            return response;


        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request)
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
