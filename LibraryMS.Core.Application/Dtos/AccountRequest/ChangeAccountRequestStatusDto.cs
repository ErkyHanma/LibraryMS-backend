namespace LibraryMS.Core.Application.Dtos.AccountRequest
{
    public class ChangeAccountRequestStatusDto
    {
        public required string Status { get; set; }
        public string? RejectionReason { get; set; }
    }
}
