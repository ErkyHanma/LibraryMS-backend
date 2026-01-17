using LibraryMS.Core.Application.Dtos.Auth;

namespace LibraryMS.Core.Application.Interfaces
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