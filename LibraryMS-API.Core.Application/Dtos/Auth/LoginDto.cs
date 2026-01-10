namespace LibraryMS_API.Core.Application.Dtos.Auth
{
    public class LoginDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
