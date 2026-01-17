using LibraryMS.Core.Application.Dtos.Base;
using LibraryMS.Core.Application.Dtos.BorrowRecord;

namespace LibraryMS.Core.Application.Interfaces
{
    public interface IBorrowRecordService
    {
        Task<PaginatedResult<BorrowRecordDto>> GetAllAsync(string? searchTerm, string? status, string? order = "desc", int page = 1, int limit = 10);
        Task<PaginatedResult<BorrowRecordDto>> GetAllByUserIdAsync(string userId, string? searchTerm, string? status, string? order = "desc", int page = 1, int limit = 10);
        Task<BorrowRecordDto?> GetById(int borrowRecordId);
        Task<BorrowRecordDto?> AddBorrowRecordAsync(AddBorrowRecordDto dto);
        Task<bool> ReturnBorrowedRecordAsync(int borrowRecordId);
    }
}