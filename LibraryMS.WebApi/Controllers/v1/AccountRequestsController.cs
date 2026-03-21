using Asp.Versioning;
using LibraryMS.Core.Application.Dtos.AccountRequest;
using LibraryMS.Core.Application.Interfaces;
using LibraryMS.Core.Domain.Common.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryMS.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize(Roles = $"{nameof(Roles.Admin)}")]
    public class AccountRequestsController : BaseController
    {

        private readonly IAccountRequestService _accountRequestService;

        public AccountRequestsController(IAccountRequestService accountRequestService)
        {
            _accountRequestService = accountRequestService;
        }


        // GET
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccountRequestDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllAccountRequest(
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] string? order,
            [FromQuery] int page,
            [FromQuery] int limit
            )
        {
            var accountRequest = await _accountRequestService.GetAllAsync(search, status, order, page, limit);
            return Ok(accountRequest);
        }

        // GET
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccountRequestDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBookById(int id)
        {
            var accountRequest = await _accountRequestService.GetByIdAsync(id);

            if (accountRequest == null)
            {
                return NotFound($"Account Request with ID {id} not found");
            }

            return Ok(accountRequest);

        }

        // PATCH
        [HttpPatch("{id}/change-status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangeAccountRequestStatus(int id, [FromBody] ChangeAccountRequestStatusDto dto)
        {
            if (!Enum.TryParse<AccountRequestStatus>(dto.Status, ignoreCase: true, out var statusEnum))
            {
                return BadRequest(new { message = $"Invalid request status '{dto.Status}'" });
            }

            var accountRequest = await _accountRequestService.ChangeRequestStatusAsync(id, statusEnum, dto.UserId, dto.RejectionReason);

            if (!accountRequest)
                return BadRequest($"Error changing account request status");


            return Ok(accountRequest);

        }
    }
}
