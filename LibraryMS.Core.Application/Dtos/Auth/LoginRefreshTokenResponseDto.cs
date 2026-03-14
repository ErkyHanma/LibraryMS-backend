namespace LibraryMS.Core.Application.Dtos.Auth
{
    public class LoginRefreshTokenResponseDto
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
    }
}
