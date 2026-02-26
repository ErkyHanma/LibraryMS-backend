using LibraryMS.Core.Application.Dtos.Base;
using LibraryMS.Core.Application.Dtos.Book;

namespace LibraryMS.Core.Application.Interfaces
{
    public interface IBookService
    {
        Task<PaginatedResult<BookDto>> GetAllAsync(string? searchTerm, List<string>? categories, string? order = "desc", bool? isAvailable = false, int page = 1, int limit = 10);
        Task<PaginatedResult<BookDto>> GetAllByCategoryIdAsync(int id, int page = 1, int limit = 10);
        Task<BookDto?> GetByIdAsync(int id);
        Task<BookDto?> AddAsync(AddBookDto dto);
        Task<BookDto?> EditAsync(int id, EditBookDto dto);
        Task<bool> DeleteAsync(int id);
    }
}