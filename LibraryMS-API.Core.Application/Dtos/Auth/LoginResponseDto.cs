namespace LibraryMS_API.Core.Application.Dtos.Auth
{
    public class LoginResponseDto
    {
        public required AuthUserDto User { get; set; }
        public string? AccessToken { get; set; }

    }
}
