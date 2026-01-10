using LibraryMS_API.Core.Domain.Entities;
using LibraryMS_API.Infrastructure.Persistence.Repositories.Base;

namespace LibraryMS_API.Core.Domain.Interfaces.Repositories
{
    public interface IBookRepository : IGenericRepository<Book>
    {
        Task<Book?> AddBookWithCategories(Book entity, List<int> categoryIds);
        Task<Book?> EditBookWithCategories(Book newEntity, int id, List<int> categoryIds);
    }
}
