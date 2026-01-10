using LibraryMS_API.Core.Domain.Common.Enum;

namespace LibraryMS_API.Core.Application.Dtos.User
{
    public class UserDto
    {
        public required string Id { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public UserStatus Status { get; set; }
        public required string UniversityId { get; set; }
        public required string ProfileImageUrl { get; set; }
        public required Roles Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? JoinedAt { get; set; }
    }
}
