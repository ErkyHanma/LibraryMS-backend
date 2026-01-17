using LibraryMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryMS.Infrastructure.Persistence.Contexts.EntityConfiguration
{
    public class BorrowRecordEntityConfiguration : IEntityTypeConfiguration<BorrowRecord>
    {
        public void Configure(EntityTypeBuilder<BorrowRecord> builder)
        {
            #region Basic configuration
            builder.HasKey(br => br.BorrowRecordId);
            builder.ToTable("BorrowRecords");
            #endregion

            #region Property configurations

            #endregion

            #region Relationshipsz
            #endregion

            #region Indexes
            #endregion
        }
    }
}
