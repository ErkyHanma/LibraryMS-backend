using LibraryMS.Core.Application.Dtos.User;
using LibraryMS.Core.Domain.Common.Enum;

namespace LibraryMS.Core.Application.Dtos.AccountRequest
{
    public class AccountRequestDto
    {
        public int AccountRequestId { get; set; }
        public required UserDto User { get; set; }
        public AccountRequestStatus Status { get; set; }
        public string? RejectionReason { get; set; }
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
