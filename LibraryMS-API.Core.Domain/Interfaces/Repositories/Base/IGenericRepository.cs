namespace LibraryMS_API.Infrastructure.Persistence.Repositories.Base
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task<List<TEntity>> GetAllListAsync();
        Task<List<TEntity>> GetAllListWithIncludeAsync(List<string> properties);
        IQueryable<TEntity> GetAllQuery();
        IQueryable<TEntity> GetAllQueryWithInclude(List<string> properties);
        Task<TEntity?> GetByIdAsync(int id);
        Task<TEntity?> GetByIdWithIncludeAsync(int id, List<string> properties);
        Task<TEntity?> AddAsync(TEntity entity);
        Task<List<TEntity>?> AddRangeAsync(List<TEntity> entities);
        Task<TEntity?> EditAsync(int id, TEntity entity);
        Task DeleteAsync(int id);
    }
}