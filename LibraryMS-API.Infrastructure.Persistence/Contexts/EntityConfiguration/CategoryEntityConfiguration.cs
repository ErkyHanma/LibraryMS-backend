using LibraryMS_API.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryMS_API.Infrastructure.Persistence.Contexts.EntityConfiguration
{
    public class CategoryEntityConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {

            #region Basic configuration
            builder.ToTable("Categories");
            builder.HasKey(c => c.CategoryId);
            #endregion

            #region Property configurations
            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(255);

            builder.HasIndex(c => c.Name)
                .IsUnique();
            #endregion

            #region Relationships
            #endregion

            #region Indexes
            #endregion






        }
    }
}
