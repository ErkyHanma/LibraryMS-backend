using LibraryMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryMS.Infrastructure.Persistence.Contexts.EntityConfiguration
{
    public class BookCategoryEntityConfiguration : IEntityTypeConfiguration<BookCategory>
    {
        public void Configure(EntityTypeBuilder<BookCategory> builder)
        {
            #region Basic configuration
            builder.HasKey(bc => new { bc.BookId, bc.CategoryId });
            builder.ToTable("BookCategories");
            #endregion

            #region Property configurations

            #endregion

            #region Relationships
            builder.HasOne(bc => bc.Book)
              .WithMany(b => b.BookCategories)
              .HasForeignKey(bc => bc.BookId)
              .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(bc => bc.Category)
                .WithMany(c => c.BookCategories)
                .HasForeignKey(bc => bc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion

            #region Indexes
            #endregion

        }
    }
}
