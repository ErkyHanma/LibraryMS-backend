namespace LibraryMS.Core.Application.Dtos.Auth
{
    public class SignUpResponseDto
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string UniversityId { get; set; }
        public List<string>? Roles { get; set; }

    }
}
