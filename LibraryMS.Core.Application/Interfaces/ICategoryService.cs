using LibraryMS.Core.Application.Dtos.Base;
using LibraryMS.Core.Application.Dtos.Category;

namespace LibraryMS.Core.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllAsync();
        Task<PaginatedResult<CategoryDto>> GetAllWithPaginationAsync(string? search, string? order = "desc", int page = 1, int limit = 10);
        Task<List<CategoryDto>> GetPopularCategoriesAsync(int limit = 10);
        Task<CategoryDto?> GetByIdAsync(int id);
        Task<CategoryDto?> AddAsync(AddCategoryDto dto);
        Task<bool> DeleteAsync(int id);
        Task<CategoryDto?> EditAsync(int id, EditCategoryDto dto);
    }
}