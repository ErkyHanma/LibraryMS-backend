using System.ComponentModel.DataAnnotations;

namespace LibraryMS_API.Core.Application.Dtos.Auth
{
    public class ResetPasswordRequestDto
    {
        public required string Id { get; set; }

        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "Password must match")]
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }
        public required string Token { get; set; }
    }
}
