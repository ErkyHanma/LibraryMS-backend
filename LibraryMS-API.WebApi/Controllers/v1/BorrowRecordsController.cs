using Asp.Versioning;
using LibraryMS_API.Core.Application.Dtos.BorrowRecord;
using LibraryMS_API.Core.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryMS_API.WebApi.Controllers.v1
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BorrowRecordDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllBorrowRecord(
            [FromQuery] int page,
            [FromQuery] int limit,
            [FromQuery] string? search,
            [FromQuery] string? status)

        {
            var dtoList = await _borrowRecordService.GetAllAsync(search, status, page, limit);
            return Ok(dtoList);

        }

        // GET
        [HttpGet("user/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BorrowRecordDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllBorrowRecordByUserId(
            string userId,
            [FromQuery] int page,
            [FromQuery] int limit,
            [FromQuery] string? search,
            [FromQuery] string? status)

        {
            var dtoList = await _borrowRecordService.GetAllByUserIdAsync(userId, search, status, page, limit);
            return Ok(dtoList);
        }

        // GET
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BorrowRecordDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBorrowRecordById(int id)
        {
            var dto = await _borrowRecordService.GetById(id);

            if (dto == null)
                return NotFound($"Borrow record with ID {id} not found");

            return Ok(dto);
        }


        [HttpPost]
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

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReturnBorrowRecord(int id)
        {
            var isSuccess = await _borrowRecordService.ReturnBorrowedRecordAsync(id);

            if (!isSuccess)
                return BadRequest("Failed to return borrow record.");

            return Ok();

        }
    }
}
