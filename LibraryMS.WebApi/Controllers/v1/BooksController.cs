using Asp.Versioning;
using LibraryMS.Core.Application.Dtos.Book;
using LibraryMS.Core.Application.Interfaces;
using LibraryMS.Core.Domain.Common.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryMS.WebApi.Controllers.v1
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
            [FromQuery] List<string>? category,
            [FromQuery] string? order,
            [FromQuery] bool? isAvailable,
            [FromQuery] int page,
            [FromQuery] int limit
            )
        {

            var categories = category ?? [];

            var books = await _bookService.GetAllAsync(search, categories, order, isAvailable, page, limit);
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
                return Problem(
                    title: "Book not found",
                    detail: $"Book with ID {id} not found",
                    statusCode: StatusCodes.Status404NotFound
                );


            return Ok(book);

        }

        // GET
        [HttpGet("category/{id}")]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.User)}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllBookByCategoryId(
            int id,
            [FromQuery] int page,
            [FromQuery] int limit)
        {
            var books = await _bookService.GetAllByCategoryIdAsync(id, page, limit);
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


        [HttpPut("{id}")]
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
