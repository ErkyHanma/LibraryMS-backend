using ArtemisBanking.Core.Application.Dtos.Auth;
using LibraryMS_API.Core.Application.Dtos.Auth;

namespace LibraryMS_API.Core.Application.Interfaces
{
    public interface IAuthService
    {
        Task<string> ConfirmAccountAsync(string userId, string token);
        Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request);
        Task<LoginResponseDto> LoginAsync(LoginDto loginDto);
        Task<ForgotPasswordResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request);
        Task<SignUpResponseDto> SignUpAsync(SignUpDto dto);
    }
}