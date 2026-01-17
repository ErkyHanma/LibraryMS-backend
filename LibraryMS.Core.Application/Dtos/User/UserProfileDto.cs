using LibraryMS.Core.Domain.Common.Enum;

namespace LibraryMS.Core.Application.Dtos.User
{
    public class UserProfileDto
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string ProfileImageUrl { get; set; }
        public required Roles Role { get; set; }
        public required string UniversityId { get; set; }
        public UserStatus Status { get; set; }
        public DateTime? JoinedAt { get; set; }

        // Statistics
        public int TotalBorrowed { get; set; }
        public int CurrentlyActive { get; set; }
        public int OverdueBooks { get; set; }
        public int MaxAllowedBooks { get; set; } = 5;
    }
}
