namespace LibraryMS.Core.Application.Dtos.Category
{
    public class EditCategoryDto
    {
        public required string Name { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
