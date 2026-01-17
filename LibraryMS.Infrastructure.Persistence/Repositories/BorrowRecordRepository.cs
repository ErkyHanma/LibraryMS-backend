using LibraryMS.Core.Application.Exceptions;
using LibraryMS.Core.Domain.Entities;
using LibraryMS.Core.Domain.Interfaces.Repositories;
using LibraryMS.Infrastructure.Persistence.Contexts;
using LibraryMS.Infrastructure.Persistence.Repositories.Base;
using System.Net;

namespace LibraryMS.Infrastructure.Persistence.Repositories
{
    public class BorrowRecordRepository : GenericRepository<BorrowRecord>, IBorrowRecordRepository
    {

        private readonly LibraryMSContext _context;
        public BorrowRecordRepository(LibraryMSContext context) : base(context)
        {
            _context = context;
        }

        public override async Task<BorrowRecord?> AddAsync(BorrowRecord entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "Borrow record cannot be null");

            try
            {
                var book = await _context.Set<Book>().FindAsync(entity.BookId);

                if (book == null)
                    throw ApiException.NotFound(
                        $"Book with ID {entity.BookId} not found");

                if (book.AvailableCopies <= 0)
                    throw ApiException.BadRequest(
                        $"Cannot borrow book '{book.Title}' (ID: {entity.BookId}): No copies available");

                await _context.Set<BorrowRecord>().AddAsync(entity);
                book.AvailableCopies -= 1;

                await _context.SaveChangesAsync();

                return entity;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApiException(
                    $"Unexpected error creating borrow record for user '{entity.UserId}' and book ID {entity.BookId}: {ex.Message}",
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        public async Task<bool> ReturnBorrowedRecordAsync(int borrowedRecordId)
        {
            try
            {
                var borrowedRecord = await _context.Set<BorrowRecord>().FindAsync(borrowedRecordId);

                if (borrowedRecord == null)
                    throw ApiException.NotFound($"Borrowed record with ID {borrowedRecordId} not found");

                if (borrowedRecord.ReturnDate != null)
                    throw ApiException.BadRequest($"This borrowed record is already returned.");

                var borrowedbook = await _context.Set<Book>().FindAsync(borrowedRecord.BookId);

                if (borrowedbook == null)
                    throw ApiException.NotFound($"Borrowed Book not found");

                borrowedRecord.ReturnDate = DateTime.UtcNow;
                borrowedbook.AvailableCopies += 1;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApiException(
                    $"Unexpected error returning borrow record: {ex.Message}",
                    (int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
