namespace LibraryMS.Core.Domain.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }
        public required string Name { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public ICollection<BookCategory>? BookCategories { get; set; }


    }
}
