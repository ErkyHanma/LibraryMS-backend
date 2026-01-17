using LibraryMS.Core.Application.Dtos.Category;

namespace LibraryMS.Core.Application.Dtos.Book
{
    public class BookDto
    {
        public int BookId { get; set; }
        public required string Title { get; set; }
        public required string Author { get; set; }
        public required List<CategoryDto> Categories { get; set; }
        public required string Description { get; set; }
        public required string Summary { get; set; }
        public int Pages { get; set; }
        public DateTime PublishDate { get; set; }
        public required string CoverUrl { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

    }
}
