using LibraryMS.Core.Domain.Entities;
using LibraryMS.Infrastructure.Persistence.Repositories.Base;

namespace LibraryMS.Core.Domain.Interfaces.Repositories
{
    public interface IBorrowRecordRepository : IGenericRepository<BorrowRecord>
    {
        Task<bool> ReturnBorrowedRecordAsync(int borrowedRecordId);
    }
}
