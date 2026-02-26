using LibraryMS.Core.Domain.Common.Enum;
using LibraryMS.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibraryMS.Infrastructure.Identity.Seeds
{
    public static class DefaultAdminUser
    {
        public static async Task SeedAsync(UserManager<User> userManager)
        {
            var user = new User
            {
                Name = "Carlos",
                LastName = "Rodriguez",
                UserName = "DefaultAdmin",
                Email = "admin@example.com",
                EmailConfirmed = true,
                JoinedAt = DateTime.UtcNow,
                Status = UserStatus.Approved,
                UniversityId = "2026-0000",
                CreatedAt = DateTime.UtcNow

            };

            if (await userManager.Users.AllAsync(u => u.Id != user.Id))
            {
                if (!await userManager.Users
                    .AnyAsync(u => u.Email == user.Email || u.UniversityId == user.UniversityId))
                {
                    await userManager.CreateAsync(user, "Pa$$word123");
                    await userManager.AddToRoleAsync(user, Roles.Admin.ToString());
                }
            }
        }
    }
}