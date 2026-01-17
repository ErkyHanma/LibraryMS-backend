using Asp.Versioning;
using LibraryMS_API.Core.Application.Dtos.Book;
using LibraryMS_API.Core.Application.Dtos.Category;
using LibraryMS_API.Core.Application.Interfaces;
using LibraryMS_API.Core.Domain.Common.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryMS_API.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    public class CategoriesController : BaseController
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET
        [HttpGet]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.User)}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(categories);
        }

        // GET
        [HttpGet("{id}")]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.User)}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CategoryDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);

            if (category == null)
            {
                return NotFound($"Category with ID {id} not found");
            }

            return Ok(category);

        }

        [HttpPost]
        [Authorize(Roles = $"{nameof(Roles.Admin)}")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CategoryDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddCategory([FromBody] AddCategoryDto dto)
        {
            var addedCategory = await _categoryService.AddAsync(dto);

            if (addedCategory == null)
                return BadRequest("Failed to add category.");

            return StatusCode(StatusCodes.Status201Created, addedCategory);

        }


        [HttpPut("{id}")]
        [Authorize(Roles = $"{nameof(Roles.Admin)}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CategoryDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EditCategory(int id, [FromBody] EditCategoryDto dto)
        {
            var editedCategory = await _categoryService.EditAsync(id, dto);

            if (editedCategory == null)
                return BadRequest("Failed to edit category.");

            return Ok(editedCategory);

        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{nameof(Roles.Admin)}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var isDeleted = await _categoryService.DeleteAsync(id);

            if (!isDeleted)
                return BadRequest("Failed to delete category.");

            return NoContent();

        }

    }
}
