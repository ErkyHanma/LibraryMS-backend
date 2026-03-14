namespace LibraryMS.Core.Application.Dtos.Auth
{
    public class LoginResponseDto
    {
        public required AuthUserDto User { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }

    }
}
