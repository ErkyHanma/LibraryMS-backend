using Asp.Versioning;
using LibraryMS_API.Core.Application.Dtos.Book;
using LibraryMS_API.Core.Application.Interfaces;
using LibraryMS_API.Core.Domain.Common.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryMS_API.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    public class BooksController : BaseController
    {
        private readonly IBookService _bookService;
        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        // GET
        [HttpGet]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.User)}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllBooks(
            [FromQuery] string? search,
            [FromQuery] string? category,
            [FromQuery] string? order,
            [FromQuery] bool? isAvailable,
            [FromQuery] int page,
            [FromQuery] int limit
            )
        {
            var books = await _bookService.GetAllAsync(search, category, order, isAvailable, page, limit);
            return Ok(books);
        }

        // GET
        [HttpGet("{id}")]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.User)}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBookById(int id)
        {
            var book = await _bookService.GetByIdAsync(id);

            if (book == null)
            {
                return NotFound($"Book with ID {id} not found");
            }

            return Ok(book);

        }

        // GET
        [HttpGet("category/{id}")]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.User)}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllBookByCategoryId(int id)
        {
            var books = await _bookService.GetAllByCategoryIdAsync(id);

            if (books.Count <= 0)
            {
                return NoContent();
            }

            return Ok(books);

        }


        [HttpPost]
        [Authorize(Roles = $"{nameof(Roles.Admin)}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(BookDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddBook([FromForm] AddBookDto dto)
        {
            var addedBook = await _bookService.AddAsync(dto);

            if (addedBook == null)
                return BadRequest("Failed to add book.");


            return StatusCode(StatusCodes.Status201Created, addedBook);

        }


        [HttpPost("{id}")]
        [Authorize(Roles = $"{nameof(Roles.Admin)}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EditBook(int id, [FromForm] EditBookDto dto)
        {
            var editedBook = await _bookService.EditAsync(id, dto);

            if (editedBook == null)
                return BadRequest("Failed to edit book.");

            return Ok(editedBook);

        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{nameof(Roles.Admin)}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var isDeleted = await _bookService.DeleteAsync(id);

            if (!isDeleted)
                return BadRequest("Failed to delete book.");

            return NoContent();

        }
    }
}
