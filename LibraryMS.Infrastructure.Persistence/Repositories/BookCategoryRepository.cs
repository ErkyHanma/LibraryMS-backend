using LibraryMS.Core.Domain.Entities;
using LibraryMS.Core.Domain.Interfaces.Repositories;
using LibraryMS.Infrastructure.Persistence.Contexts;
using LibraryMS.Infrastructure.Persistence.Repositories.Base;

namespace LibraryMS.Infrastructure.Persistence.Repositories
{
    public class BookCategoryRepository : GenericRepository<BookCategory>, IBookCategoryRepository
    {
        public BookCategoryRepository(LibraryMSContext context) : base(context) { }
    }
}
