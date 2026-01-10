using LibraryMS_API.Core.Domain.Entities;
using LibraryMS_API.Infrastructure.Persistence.Repositories.Base;

namespace LibraryMS_API.Core.Domain.Interfaces.Repositories
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
    }
}
