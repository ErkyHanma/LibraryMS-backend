namespace LibraryMS.Core.Application.Dtos.BorrowRecord
{
    public class AddBorrowRecordDto
    {
        public required string UserId { get; set; }
        public int BookId { get; set; }
        public DateTime BorrowDate { get; set; }
    }
}
