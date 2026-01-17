namespace LibraryMS.Core.Application.Dtos.Base
{
    public abstract class BaseResponseDto
    {
        public bool HasError { get; set; }
        public required List<string> Errors { get; set; }
    }
}
