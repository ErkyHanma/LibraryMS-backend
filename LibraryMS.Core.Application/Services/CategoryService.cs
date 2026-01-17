using AutoMapper;
using LibraryMS.Core.Application.Dtos.Category;
using LibraryMS.Core.Application.Interfaces;
using LibraryMS.Core.Domain.Entities;
using LibraryMS.Core.Domain.Interfaces.Repositories;

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
