using LibraryMS.Core.Domain.Common.Enum;
using LibraryMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryMS.Infrastructure.Persistence.Contexts.EntityConfiguration
{
    public class AccountRequestEntityConfiguration : IEntityTypeConfiguration<AccountRequest>
    {
        public void Configure(EntityTypeBuilder<AccountRequest> builder)
        {
            #region Basic configuration
            builder.HasKey(ar => ar.AccountRequestId);
            builder.ToTable("AccountRequests");
            #endregion

            #region Property configurations
            builder.Property(ar => ar.Status)
                .IsRequired()
                .HasDefaultValue(AccountRequestStatus.Pending);
            builder.Property(ar => ar.RejectionReason)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.HasQueryFilter(ar => ar.DeletedAt == null);

            #endregion

            #region Relationships
            #endregion

            #region Indexes
            #endregion
        }
    }
}
