using LibraryMS.Core.Application.Dtos.AccountRequest;
using LibraryMS.Core.Application.Dtos.Base;
using LibraryMS.Core.Domain.Common.Enum;

namespace LibraryMS.Core.Application.Interfaces
{
    public interface IAccountRequestService
    {
        Task<bool> ChangeRequestStatusAsync(int accountRequestId, AccountRequestStatus status, string? rejectionReason);
        Task<PaginatedResult<AccountRequestDto>> GetAllAsync(string? status, string? order = "desc", int page = 1, int limit = 10);
        Task<AccountRequestDto?> GetByIdAsync(int id);
    }
}