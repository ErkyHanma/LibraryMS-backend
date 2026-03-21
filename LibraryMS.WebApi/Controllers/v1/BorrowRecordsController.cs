using Asp.Versioning;
using LibraryMS.Core.Application.Dtos.BorrowRecord;
using LibraryMS.Core.Application.Interfaces;
using LibraryMS.Core.Domain.Common.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryMS.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    public class BorrowRecordsController : BaseController
    {

        private readonly IBorrowRecordService _borrowRecordService;
        public BorrowRecordsController(IBorrowRecordService borrowRecordService)
        {
            _borrowRecordService = borrowRecordService;
        }

        // GET
        [HttpGet]
        [Authorize(Roles = $"{nameof(Roles.Admin)}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BorrowRecordDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllBorrowRecord(
            [FromQuery] int page,
            [FromQuery] int limit,
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] string? order)

        {
            var dtoList = await _borrowRecordService.GetAllAsync(search, status, order, page, limit);
            return Ok(dtoList);

        }

        // GET
        [HttpGet("user/{userId}")]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.User)}, {nameof(Roles.Demo)}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BorrowRecordDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllBorrowRecordByUserId(
            string userId,
            [FromQuery] int page,
            [FromQuery] int limit,
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] string? order)

        {
            var dtoList = await _borrowRecordService.GetAllByUserIdAsync(userId, search, status, order, page, limit);
            return Ok(dtoList);
        }

        // GET
        [HttpGet("{id}")]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.User)}, {nameof(Roles.Demo)}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BorrowRecordDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBorrowRecordById(int id)
        {
            var dto = await _borrowRecordService.GetById(id);

            if (dto == null)
                return NotFound($"Borrow record with ID {id} not found");

            return Ok(dto);
        }


        // POST
        [HttpPost]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.User)}")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(BorrowRecordDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddBorrowRecord([FromBody] AddBorrowRecordDto dto)
        {

            var addedBorrowRecord = await _borrowRecordService.AddBorrowRecordAsync(dto);

            if (addedBorrowRecord == null)
            {
                return BadRequest("Failed to borrow record.");
            }

            return StatusCode(StatusCodes.Status201Created, addedBorrowRecord);

        }

        // PATCH
        [HttpPatch("{id}/return")]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.User)}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReturnBorrowRecord(int id)
        {
            var isSuccess = await _borrowRecordService.ReturnBorrowedRecordAsync(id);
            if (!isSuccess)
                return BadRequest("Failed to return borrow record.");

            return Ok(new { message = "Book returned successfully." });
        }
    }
}
