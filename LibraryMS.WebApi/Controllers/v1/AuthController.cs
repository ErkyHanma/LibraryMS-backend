using Asp.Versioning;
using LibraryMS.Core.Application.Dtos.Auth;
using LibraryMS.Core.Application.Dtos.User;
using LibraryMS.Core.Application.Interfaces;
using LibraryMS.Core.Domain.Common.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryMS.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public AuthController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponseDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            return Ok(await _authService.LoginAsync(dto));
        }

        [HttpPost("sign-up")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponseDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SignUp([FromBody] SignUpDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            return Ok(await _authService.SignUpAsync(dto));
        }

        [HttpGet("me")]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.User)}, {nameof(Roles.Demo)}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUser()
        {
            var currentUserId = User.FindFirst("uid")?.Value;

            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var user = await _userService.GetById(currentUserId);

            if (user == null)
                return NotFound("User not found");

            return Ok(user);
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginRefreshTokenResponseDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LoginWithRefreshToken([FromBody] LoginRefreshTokenRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            return Ok(await _authService.LoginUserWithRefreshTokenAsync(dto.RefreshToken));
        }

        [HttpPost("revoke")]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.User)}, {nameof(Roles.Demo)}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Revoke()
        {
            var currentUserId = User.FindFirst("uid")?.Value;

            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();


            return await _authService.RevokeRefreshTokenAsync(currentUserId) ? NoContent() : BadRequest();

        }

    }
}


