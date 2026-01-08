using LibraryMS_API.Core.Domain.Common.Enum;
using LibraryMS_API.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibraryMS_API.Infrastructure.Identity.Seeds
{
    public class DefaultUser
    {
        public static async Task SeedAsync(UserManager<User> userManager)
        {
            var user = new User
            {
                FullName = "Edward Elric",
                UserName = "DefaultUser",
                Email = "user@example.com",
                EmailConfirmed = true,
                JoinedAt = DateTime.UtcNow,
                Status = UserStatus.Approved,
                UniversityId = "2026-0001",

            };

            if (await userManager.Users.AllAsync(u => u.Id != user.Id))
            {
                if (!await userManager.Users
                    .AnyAsync(u => u.Email == user.Email || u.UniversityId == user.UniversityId))
                {
                    await userManager.CreateAsync(user, "Pa$$word123");
                    await userManager.AddToRoleAsync(user, Roles.User.ToString());
                }
            }
        }
    }
}