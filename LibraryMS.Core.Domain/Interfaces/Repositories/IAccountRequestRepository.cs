using LibraryMS.Core.Domain.Common.Enum;
using LibraryMS.Core.Domain.Entities;
using LibraryMS.Infrastructure.Persistence.Repositories.Base;

namespace LibraryMS.Core.Domain.Interfaces.Repositories
{
    public interface IAccountRequestRepository : IGenericRepository<AccountRequest>
    {
        Task<AccountRequest?> ChangeStatus(int AccountRequestId, AccountRequestStatus status, string? rejectionReason);
    }
}
