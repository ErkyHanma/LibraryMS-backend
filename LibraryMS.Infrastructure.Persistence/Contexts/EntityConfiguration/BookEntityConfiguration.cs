using LibraryMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryMS.Infrastructure.Persistence.Contexts.EntityConfiguration
{
    public class BookEntityConfiguration : IEntityTypeConfiguration<Book>
    {
        public void Configure(EntityTypeBuilder<Book> builder)
        {

            #region Basic configuration
            builder.HasKey(b => b.BookId);
            builder.ToTable("Books");
            #endregion

            #region Property configurations

            builder.Property(b => b.Title)
                .HasMaxLength(100);

            builder.Property(b => b.Author)
                .HasMaxLength(100);

            builder.Property(b => b.Description)
                .HasMaxLength(1000);

            builder.Property(b => b.Summary)
                .HasMaxLength(2000);

            builder.Property(b => b.CoverImageUrl)
                .HasMaxLength(500);

            builder.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.Property(b => b.UpdatedAt)
                .IsRequired(false);

            builder.Property(b => b.AvailableCopies)
                .HasDefaultValue(0);

            builder.HasQueryFilter(b => b.DeletedAt == null);

            #endregion

            #region relationships
            builder.HasMany(b => b.BorrowRecords)
                .WithOne(br => br.Book)
                .HasForeignKey(br => br.BookId)
                .OnDelete(DeleteBehavior.Restrict);


            #endregion

            #region Indexes
            #endregion

        }
    }
}
