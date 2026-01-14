namespace LibraryMS_API.Core.Application.Dtos.Auth
{
    public class SignUpDto
    {
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string UniversityId { get; set; }

    }
}
