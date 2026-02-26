namespace LibraryMS.Core.Application.Dtos.Category
{
    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public required string Name { get; set; }
        public int BooksCount { get; set; } // Total created book related with this category
        public required DateTime CreatedAt { get; set; }
    }
}
