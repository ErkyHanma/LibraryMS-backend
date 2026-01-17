using LibraryMS.Core.Domain.Entities;
using LibraryMS.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

public static class DefaultCategory
{
    public static async Task SeedAsync(LibraryMSContext context, string[] categories)
    {
        foreach (var categoryName in categories)
        {
            var exists = await context.Categories
                .AnyAsync(c => c.Name == categoryName);

            if (!exists)
            {
                context.Categories.Add(new Category { Name = categoryName });
            }
        }

        await context.SaveChangesAsync();
    }
}