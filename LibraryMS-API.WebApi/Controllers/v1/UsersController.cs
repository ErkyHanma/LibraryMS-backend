using Asp.Versioning;
using LibraryMS_API.Core.Application.Dtos.User;
using LibraryMS_API.Core.Application.Interfaces;
using LibraryMS_API.Core.Domain.Common.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryMS_API.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    public class UsersController : BaseController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }


        [HttpGet]
        [Authorize(Roles = $"{nameof(Roles.Admin)}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserListDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] string? search,
            [FromQuery] string? order,
            [FromQuery] int page,
            [FromQuery] int limit
            )
        {
            var users = await _userService.GetAllWithBorrowBookAsync(search, order, page, limit);
            return Ok(users);
        }



        [HttpGet("{id}")]
        [Authorize(Roles = $"{nameof(Roles.Admin)}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userService.GetById(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpGet("{id}/profile")]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.User)}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserProfileDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserProfileById(string id)
        {
            var user = await _userService.GetProfileById(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPut("{id}/profile")]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.User)}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EditUser(string id, [FromForm] EditUserDto dto)
        {
            var user = await _userService.EditUserAsync(id, dto);

            if (user == null)
                return BadRequest("Failed to edit user.");

            return Ok(user);
        }


        [HttpPatch("{id}/role")]
        [Authorize(Roles = $"{nameof(Roles.Admin)}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangeUserRole(string id, [FromBody] ChangeRoleDto dto)
        {
            if (!Enum.TryParse<Roles>(dto.Role, ignoreCase: true, out var roleEnum))
            {
                return BadRequest(new { message = $"Invalid role '{dto.Role}'" });
            }

            var success = await _userService.ChangeRole(id, roleEnum);
            if (!success)
                return NotFound($"User with ID {id} not found");

            return Ok("User role updated successfully");
        }



        [HttpPatch("{id}/change-status")]
        [Authorize(Roles = $"{nameof(Roles.Admin)}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangeStatus(string id, ChangeStatusDto dto)
        {

            if (!Enum.TryParse<UserStatus>(dto.Status, ignoreCase: true, out var statusEnum))
            {
                return BadRequest($"Invalid status {dto.Status}");
            }

            var success = await _userService.ChangeStatus(id, statusEnum);
            if (!success)
                return NotFound($"User with ID {id} not found");

            return Ok("User status change successfully");
        }

    }
}
