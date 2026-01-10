namespace LibraryMS_API.Core.Application.Dtos.Auth
{
    public class SignUpDto
    {
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string UniversityId { get; set; }

    }
}
