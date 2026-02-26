using AutoMapper;
using LibraryMS.Core.Application.Dtos.Base;
using LibraryMS.Core.Application.Dtos.Category;
using LibraryMS.Core.Application.Interfaces;
using LibraryMS.Core.Domain.Entities;
using LibraryMS.Core.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LibraryMS.Core.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IValidationService _validationService;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, IValidationService validationService, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _validationService = validationService;
            _mapper = mapper;
        }

        public async Task<List<CategoryDto>> GetAllAsync()
        {
            var listEntities = await _categoryRepository.GetAllListAsync();
            var listEntityDtos = _mapper.Map<List<CategoryDto>>(listEntities);

            return listEntityDtos;
        }

        public async Task<PaginatedResult<CategoryDto>> GetAllWithPaginationAsync(string? search, string? order = "desc", int page = 1, int limit = 10)
        {
            // Validate parameters
            if (page < 1) page = 1;
            if (limit < 1) limit = 10;
            if (limit > 100) limit = 100;

            var query = _categoryRepository.GetAllQueryWithInclude(["BookCategories"]);


            // Search by name
            if (!string.IsNullOrEmpty(search))
                query = query.Where(b => b.Name.ToLower().Contains(search.ToLower()));


            // Get total count for pagination
            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(total / (double)limit);


            // Apply ordering 
            query = order?.ToLower() == "asc"
                 ? query.OrderBy(c => c.CreatedAt)
                 : query.OrderByDescending(c => c.CreatedAt);


            var items = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            var listDtos = _mapper.Map<List<CategoryDto>>(items);

            return new PaginatedResult<CategoryDto>
            {
                Data = listDtos,
                Meta = new PageMetadata
                {
                    Page = page,
                    Limit = limit,
                    Total = total,
                    TotalPage = totalPages
                }
            };
        }
        public async Task<List<CategoryDto>> GetPopularCategoriesAsync(int limit = 8)
        {
            var query = _categoryRepository.GetAllQuery()
                .Include(c => c.BookCategories)
                .Where(c => c.BookCategories != null && c.BookCategories.Count > 0)
                .OrderByDescending(c => c.BookCategories!.Count);

            var items = await query.Take(limit).ToListAsync();

            var listEntityDtos = _mapper.Map<List<CategoryDto>>(items);

            return listEntityDtos;
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            var entity = await _categoryRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return null;
            }

            CategoryDto dto = _mapper.Map<CategoryDto>(entity);
            return dto;

        }

        public async Task<CategoryDto?> AddAsync(AddCategoryDto dto)
        {

            await _validationService.ValidateAsync(dto);

            Category entity = _mapper.Map<Category>(dto);
            Category? returnEntity = await _categoryRepository.AddAsync(entity);

            if (returnEntity == null)
            {
                return null;
            }

            return _mapper.Map<CategoryDto>(returnEntity);

        }

        public async Task<CategoryDto?> EditAsync(int id, EditCategoryDto dto)
        {
            Category entity = _mapper.Map<Category>(dto);
            entity.CategoryId = id;
            Category? returnEntity = await _categoryRepository.EditAsync(id, entity);

            if (returnEntity == null)
            {
                return null;
            }

            return _mapper.Map<CategoryDto>(returnEntity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await _categoryRepository.DeleteAsync(id);
            return true;
        }
    }
}
