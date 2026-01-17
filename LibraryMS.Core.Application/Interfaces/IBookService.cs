using LibraryMS.Core.Application.Dtos.Base;
using LibraryMS.Core.Application.Dtos.Book;

namespace LibraryMS.Core.Application.Interfaces
{
    public interface IBookService
    {
        Task<PaginatedResult<BookDto>> GetAllAsync(string? searchTerm, string? category, string? order = "desc", bool? isAvailable = false, int page = 1, int limit = 10);
        Task<List<BookDto>> GetAllByCategoryIdAsync(int id);
        Task<BookDto?> GetByIdAsync(int id);
        Task<BookDto?> AddAsync(AddBookDto dto);
        Task<BookDto?> EditAsync(int id, EditBookDto dto);
        Task<bool> DeleteAsync(int id);
    }
}