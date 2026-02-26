using Microsoft.AspNetCore.Http;

namespace LibraryMS.Core.Application.Dtos.Book
{
    public class EditBookDto
    {
        public required string Title { get; set; }
        public required string Author { get; set; }
        public required List<int> CategoryIds { get; set; } = [];
        public required string Description { get; set; }
        public required string Summary { get; set; }
        public int Pages { get; set; }
        public required DateTime PublishDate { get; set; }
        public IFormFile? CoverFile { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
