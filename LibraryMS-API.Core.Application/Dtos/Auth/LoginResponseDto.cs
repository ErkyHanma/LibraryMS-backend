namespace LibraryMS_API.Core.Application.Dtos.Auth
{
    public class LoginResponseDto
    {
        public required string FullName { get; set; }
        public string? AccessToken { get; set; }

    }
}
