using LibraryMS.Core.Domain.Common.Enum;
using Microsoft.AspNetCore.Identity;

namespace LibraryMS.Infrastructure.Identity.Entities
{
    public class User : IdentityUser
    {
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string UniversityId { get; set; }
        public UserStatus Status { get; set; } = UserStatus.Pending;
        public string? ProfileImageUrl { get; set; }
        public string? ProfileImageKey { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? JoinedAt { get; set; } // Date user was approved

    }
}

