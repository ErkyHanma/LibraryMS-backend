using LibraryMS_API.Core.Domain.Entities;
using LibraryMS_API.Infrastructure.Persistence.Contexts;
using LibraryMS_API.Infrastructure.Persistence.Repositories.Base;

namespace LibraryMS_API.Infrastructure.Persistence.Repositories
{
    public class BorrowRecordRepository : GenericRepository<Book>
    {
        public BorrowRecordRepository(LibraryMSContext context) : base(context) { }
    }
}
