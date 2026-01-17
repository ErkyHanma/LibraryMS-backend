namespace LibraryMS.Core.Application.Dtos.Auth
{
    public class JwtResponseDto
    {
        public bool HasError { get; set; } = false;
        public string? Error { get; set; } = null;
    }
}
