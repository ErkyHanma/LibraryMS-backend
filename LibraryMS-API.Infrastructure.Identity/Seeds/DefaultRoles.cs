using LibraryMS_API.Core.Domain.Common.Enum;
using Microsoft.AspNetCore.Identity;

namespace LibraryMS_API.Infrastructure.Identity.Seeds
{
    public static class DefaultRoles
    {
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            var roles = new[] { Roles.User, Roles.Admin };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role.ToString()))
                {
                    var identityRole = new IdentityRole
                    {
                        Name = role.ToString(),
                        NormalizedName = role.ToString().ToUpper()
                    };
                    var result = await roleManager.CreateAsync(identityRole);

                    if (!result.Succeeded)
                    {
                        throw new Exception($"Failed to create role '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }
    }
}
