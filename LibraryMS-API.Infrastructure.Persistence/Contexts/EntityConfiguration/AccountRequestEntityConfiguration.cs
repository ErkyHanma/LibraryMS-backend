using LibraryMS_API.Core.Domain.Common.Enum;
using LibraryMS_API.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryMS_API.Infrastructure.Persistence.Contexts.EntityConfiguration
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
            #endregion

            #region Relationships
            #endregion

            #region Indexes
            #endregion
        }
    }
}
