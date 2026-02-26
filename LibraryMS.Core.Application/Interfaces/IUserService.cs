using LibraryMS.Core.Application.Dtos.Base;
using LibraryMS.Core.Application.Dtos.User;
using LibraryMS.Core.Domain.Common.Enum;

namespace LibraryMS.Core.Application.Interfaces
{
    public interface IUserService
    {
        Task<PaginatedResult<UserDto>> GetAllAsync(string? search, string? status, string? order = "asc", bool? IsApproved = true, int page = 1, int limit = 10);
        Task<List<string>> GetUserIds(string? search);
        Task<PaginatedResult<UserListDto>> GetAllWithBorrowBookAsync(string? search, string? status, string? order = "asc", int page = 1, int limit = 10);
        Task<int> GetTotalUserCountAsync();
        Task<UserDto?> GetByEmail(string email);
        Task<UserDto?> GetById(string Id);
        Task<UserProfileDto?> GetProfileById(string Id);
        Task<bool> ChangeStatus(string id, UserStatus status);
        Task<bool> ChangeRole(string id, Roles role);
        Task<UserDto> EditUserAsync(string id, EditUserDto dto);
        Task DeleteAsync(string id);
    }
}