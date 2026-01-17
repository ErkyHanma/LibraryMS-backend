using LibraryMS.Core.Domain.Common.Enum;

namespace LibraryMS.Core.Domain.Entities
{
    public class AccountRequest
    {
        public int AccountRequestId { get; set; }
        public required string UserId { get; set; }
        public AccountRequestStatus Status { get; set; } = AccountRequestStatus.Pending;
        public string? RejectionReason { get; set; }
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
