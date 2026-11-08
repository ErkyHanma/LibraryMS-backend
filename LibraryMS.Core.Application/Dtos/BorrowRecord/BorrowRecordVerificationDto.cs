namespace LibraryMS.Core.Application.Dtos.BorrowRecord
{
    public class BorrowRecordVerificationDto
    {
        public int BorrowRecordId { get; set; }
        public required string ReceiptId { get; set; }
        public required string UniversityId { get; set; }
        public required string UserFullName { get; set; }
        public required string BookTitle { get; set; }
        public required string BookAuthor { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public bool IsOverdue { get; set; }
    }
}
