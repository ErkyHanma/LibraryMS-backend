namespace LibraryMS_API.Core.Domain.Entities
{
    public class BorrowRecord
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public int BookId { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }

        // Navegation properties
        public Book? Book { get; set; }
    }
}
