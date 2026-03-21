using LibraryMS.Core.Domain.Common.Enum;
using Microsoft.AspNetCore.Identity;

namespace LibraryMS.Infrastructure.Identity.Seeds
{
    public static class DefaultRoles
    {
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            var roles = new[] { Roles.User, Roles.Admin, Roles.Demo };
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
