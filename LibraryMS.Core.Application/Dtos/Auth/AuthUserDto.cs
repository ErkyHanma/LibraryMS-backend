using LibraryMS.Core.Domain.Common.Enum;

namespace LibraryMS.Core.Application.Dtos.Auth
{
    // This class represent an authenticated user with essential details only
    public class AuthUserDto
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string UniversityId { get; set; }
        public required string ProfileImageUrl { get; set; }
        public required Roles Role { get; set; }
        public required UserStatus Status { get; set; }

    }
}
