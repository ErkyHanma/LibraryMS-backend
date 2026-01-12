namespace LibraryMS_API.Core.Application.Dtos.BorrowRecord
{
    public class EditBorrowRecordDto
    {
        public required string UserId { get; set; }
        public int BookId { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}
