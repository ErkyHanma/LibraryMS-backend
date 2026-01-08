using LibraryMS_API.Core.Domain.Entities;
using LibraryMS_API.Core.Domain.Interfaces.Repositories;
using LibraryMS_API.Infrastructure.Persistence.Contexts;
using LibraryMS_API.Infrastructure.Persistence.Repositories.Base;

namespace LibraryMS_API.Infrastructure.Persistence.Repositories
{
    public class BookRepository : GenericRepository<Book>, IBookRepository
    {
        public BookRepository(LibraryMSContext context) : base(context) { }
    }
}
