namespace LibraryMS.Core.Application.Dtos.Base
{
    public class PaginatedResult<T>
    {
        public List<T> Data { get; set; } = [];
        public PageMetadata Meta { get; set; } = new();

    }
}
