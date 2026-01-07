using LibraryMS_API.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryMS_API.Infrastructure.Persistence.Contexts.EntityConfiguration
{
    public class BorrowRecordEntityConfiguration : IEntityTypeConfiguration<BorrowRecord>
    {
        public void Configure(EntityTypeBuilder<BorrowRecord> builder)
        {
            #region Basic configuration
            builder.HasKey(br => br.Id);
            builder.ToTable("BorrowRecords");
            #endregion

            #region Property configurations

            #endregion

            #region Relationships
            #endregion

            #region Indexes
            #endregion
        }
    }
}
