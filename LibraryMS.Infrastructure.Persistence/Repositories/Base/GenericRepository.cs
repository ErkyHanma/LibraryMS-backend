using LibraryMS.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LibraryMS.Infrastructure.Persistence.Repositories.Base
{
    public abstract class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {

        public LibraryMSContext _context { get; set; }
        public GenericRepository(LibraryMSContext context)
        {
            _context = context;
        }

        public virtual async Task<List<TEntity>> GetAllListAsync()
        {
            try
            {
                return await _context.Set<TEntity>().ToListAsync();

            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving the list of entities {typeof(TEntity).Name}.", ex);
            }
        }

        public virtual async Task<List<TEntity>> GetAllListWithIncludeAsync(List<string> properties)
        {
            try
            {
                var query = _context.Set<TEntity>().AsQueryable();

                foreach (var property in properties)
                {
                    query = query.Include(property);
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving the list with includes for {typeof(TEntity).Name}.", ex);
            }
        }

        public virtual async Task<TEntity?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Set<TEntity>().FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving entity {typeof(TEntity).Name} with Id {id}.", ex);
            }
        }

        public virtual IQueryable<TEntity> GetAllQuery()
        {
            try
            {
                return _context.Set<TEntity>().AsQueryable();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating the query for entity {typeof(TEntity).Name}.", ex);
            }
        }

        public virtual IQueryable<TEntity> GetAllQueryWithInclude(List<string> properties)
        {
            try
            {
                var query = _context.Set<TEntity>().AsQueryable();

                foreach (var property in properties)
                {
                    query = query.Include(property);
                }

                return query;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating query with includes for {typeof(TEntity).Name}.", ex);
            }
        }

        public virtual async Task<TEntity?> GetByIdWithIncludeAsync(int id, List<string> properties)
        {
            try
            {
                var query = _context.Set<TEntity>().AsQueryable();

                foreach (var property in properties)
                {
                    query = query.Include(property);
                }

                var keyName = $"{typeof(TEntity).Name}Id";
                return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, keyName) == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving entity {typeof(TEntity).Name} with Id {id} and includes.", ex);
            }
        }

        public virtual async Task<TEntity?> AddAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            try
            {
                await _context.Set<TEntity>().AddAsync(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding entity {typeof(TEntity).Name}.", ex);
            }
        }

        public virtual async Task<List<TEntity>?> AddRangeAsync(List<TEntity> entities)
        {
            try
            {
                await _context.Set<TEntity>().AddRangeAsync(entities);
                await _context.SaveChangesAsync();
                return entities;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public virtual async Task<TEntity?> EditAsync(int id, TEntity entity)
        {
            try
            {

                var entry = await _context.Set<TEntity>().FindAsync(id);

                if (entry != null)
                {
                    _context.Entry(entry).CurrentValues.SetValues(entity);
                    await _context.SaveChangesAsync();
                    return entry;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating entity {typeof(TEntity).Name} with Id {id}.", ex);
            }
        }

        public virtual async Task DeleteAsync(int id)
        {
            try
            {
                var entry = await _context.Set<TEntity>().FindAsync(id);

                if (entry != null)
                {
                    _context.Set<TEntity>().Remove(entry);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting entity {typeof(TEntity).Name} with Id {id}.", ex);
            }
        }
    }
}
