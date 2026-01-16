namespace LibraryMS_API.Core.Domain.Entities
{
    public class Book
    {
        public int BookId { get; set; }
        public required string Title { get; set; }
        public required string Author { get; set; }
        public required string Description { get; set; }
        public required string Summary { get; set; }
        public int Pages { get; set; }
        public DateTime PublishDate { get; set; }
        public required string CoverImageUrl { get; set; }
        public required string CoverImageKey { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navegation properties
        public ICollection<BorrowRecord>? BorrowRecords { get; set; }
        public ICollection<BookCategory> BookCategories { get; set; } = new List<BookCategory>();

    }
}


