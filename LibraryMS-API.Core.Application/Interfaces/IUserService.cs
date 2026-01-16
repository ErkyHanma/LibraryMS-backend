using LibraryMS_API.Core.Application.Dtos.Base;
using LibraryMS_API.Core.Application.Dtos.User;
using LibraryMS_API.Core.Domain.Common.Enum;

namespace LibraryMS_API.Core.Application.Interfaces
{
    public interface IUserService
    {
        Task<PaginatedResult<UserDto>> GetAllAsync(string? search, string? order = "asc", bool? IsApproved = true, int page = 1, int limit = 10);
        Task<PaginatedResult<UserListDto>> GetAllWithBorrowBookAsync(string? search, string? order = "asc", int page = 1, int limit = 10);
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