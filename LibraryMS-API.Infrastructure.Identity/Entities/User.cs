using LibraryMS_API.Core.Domain.Common.Enum;
using Microsoft.AspNetCore.Identity;

namespace LibraryMS_API.Infrastructure.Identity.Entities
{
    public class User : IdentityUser
    {
        public required string FullName { get; set; }
        public required string UniversityId { get; set; }
        public UserStatus Status { get; set; } = UserStatus.Pending;
        public DateTime JoinedAt { get; set; } // Date user was approved
    }
}

