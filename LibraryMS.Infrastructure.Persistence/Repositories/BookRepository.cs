using LibraryMS.Core.Domain.Entities;
using LibraryMS.Core.Domain.Interfaces.Repositories;
using LibraryMS.Infrastructure.Persistence.Contexts;
using LibraryMS.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace LibraryMS.Infrastructure.Persistence.Repositories
{
    public class BookRepository : GenericRepository<Book>, IBookRepository
    {

        private readonly LibraryMSContext _context;

        public BookRepository(LibraryMSContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Book?> AddBookWithCategories(Book entity, List<int> categoryIds)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));


                await _context.Set<Book>().AddAsync(entity);

                entity.BookCategories = categoryIds
                 .Select(cId => new BookCategory { BookId = entity.BookId, CategoryId = cId })
                 .ToList();

                await _context.SaveChangesAsync();

                return await _context.Books
                        .Include(b => b.BookCategories)
                        .ThenInclude(bc => bc.Category)
                        .FirstOrDefaultAsync(b => b.BookId == entity.BookId);

            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding book.", ex);
            }

        }

        public async Task<Book?> EditBookWithCategories(Book newEntity, int id, List<int> categoryIds)
        {
            try
            {
                var currentEntity = await _context.Books
                     .Include(b => b.BookCategories)
                     .FirstOrDefaultAsync(b => b.BookId == id);

                if (currentEntity == null)
                    return null;

                _context.Entry(currentEntity).CurrentValues.SetValues(newEntity);

                currentEntity.BookCategories?.Clear();
                currentEntity.BookCategories = categoryIds
                    .Select(cId => new BookCategory { BookId = id, CategoryId = cId })
                    .ToList();

                await _context.SaveChangesAsync();

                return await _context.Books
                        .Include(b => b.BookCategories)
                        .ThenInclude(bc => bc.Category)
                        .FirstOrDefaultAsync(b => b.BookId == id);

            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating book with Id {id}.", ex);
            }
        }
    }
}
