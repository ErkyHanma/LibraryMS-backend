using LibraryMS.Core.Domain.Entities;
using LibraryMS.Core.Domain.Interfaces.Repositories;
using LibraryMS.Infrastructure.Persistence.Contexts;
using LibraryMS.Infrastructure.Persistence.Repositories.Base;

namespace LibraryMS.Infrastructure.Persistence.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(LibraryMSContext context) : base(context)
        {
        }
    }
}
