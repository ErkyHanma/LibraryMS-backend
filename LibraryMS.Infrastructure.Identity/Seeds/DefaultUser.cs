using LibraryMS.Core.Domain.Common.Enum;
using LibraryMS.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LibraryMS.Infrastructure.Identity.Seeds
{
    public class DefaultUser
    {
        public static async Task SeedAsync(UserManager<User> userManager, IConfiguration config)
        {
            var user = new User
            {
                Name = "Edward",
                LastName = "Elric",
                UserName = "DefaultUser",
                Email = "user@example.com",
                EmailConfirmed = true,
                JoinedAt = DateTime.UtcNow,
                Status = UserStatus.Approved,
                UniversityId = "2026-0001",
                CreatedAt = DateTime.UtcNow

            };


            var password = config.GetValue<string>("Password:DefaultPassword");

            if (await userManager.Users.AllAsync(u => u.Id != user.Id))
            {
                if (!await userManager.Users
                    .AnyAsync(u => u.Email == user.Email || u.UniversityId == user.UniversityId) && password != null)
                {
                    await userManager.CreateAsync(user, "Pa$$word123");
                    await userManager.AddToRoleAsync(user, Roles.User.ToString());
                }
            }
        }
    }
}