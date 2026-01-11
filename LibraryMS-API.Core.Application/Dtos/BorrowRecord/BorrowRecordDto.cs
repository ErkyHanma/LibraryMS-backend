using LibraryMS_API.Core.Application.Dtos.Book;
using LibraryMS_API.Core.Application.Dtos.User;

namespace LibraryMS_API.Core.Application.Dtos.BorrowRecord
{
    public class BorrowRecordDto
    {
        public int BorrowRecordId { get; set; }
        public required UserDto User { get; set; }
        public required BookDto Book { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
