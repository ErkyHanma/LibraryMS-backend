using LibraryMS_API.Core.Domain.Common.Enum;
using LibraryMS_API.Core.Domain.Entities;
using LibraryMS_API.Infrastructure.Persistence.Repositories.Base;

namespace LibraryMS_API.Core.Domain.Interfaces.Repositories
{
    public interface IAccountRequestRepository : IGenericRepository<AccountRequest>
    {
        Task<AccountRequest?> ChangeStatus(int AccountRequestId, AccountRequestStatus status, string? rejectionReason);
    }
}
