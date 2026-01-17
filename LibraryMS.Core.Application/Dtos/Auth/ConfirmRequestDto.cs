namespace LibraryMS.Core.Application.Dtos.Auth
{
    public class ConfirmRequestDto
    {
        public required string UserId { get; set; }
        public required string Token { get; set; }

    }
}
