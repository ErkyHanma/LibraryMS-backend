namespace LibraryMS.Core.Domain.Entities
{
    public class BookCategory
    {
        public int BookId { get; set; }
        public int CategoryId { get; set; }

        // Navigation properties
        public Book? Book { get; set; }
        public Category? Category { get; set; }
    }
}
