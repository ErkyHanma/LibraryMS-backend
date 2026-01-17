using LibraryMS.Core.Application.Dtos.Category;

namespace LibraryMS.Core.Application.Interfaces
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