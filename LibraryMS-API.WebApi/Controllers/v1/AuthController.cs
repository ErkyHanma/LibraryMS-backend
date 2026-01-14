using Asp.Versioning;
using LibraryMS_API.Core.Application.Dtos.Auth;
using LibraryMS_API.Core.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryMS_API.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
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

    }
}


