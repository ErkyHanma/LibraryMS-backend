namespace LibraryMS_API.Core.Application.Dtos.Base
{
    public class PageMetadata
    {
        public int Page { get; set; }
        public int Limit { get; set; }
        public int Total { get; set; }
        public int TotalPage { get; set; }
    }
}
