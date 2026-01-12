using LibraryMS_API.Core.Application.Dtos.Category;

namespace LibraryMS_API.Core.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<CategoryDto?> AddAsync(AddCategoryDto dto);
        Task<bool> DeleteAsync(int id);
        Task<CategoryDto?> EditAsync(int id, EditCategoryDto dto);
        Task<List<CategoryDto>> GetAllAsync();
        Task<CategoryDto?> GetByIdAsync(int id);
    }
}