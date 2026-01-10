using LibraryMS_API.Core.Application.Dtos.Base;
using LibraryMS_API.Core.Application.Dtos.User;

namespace LibraryMS_API.Core.Application.Interfaces
{
    public interface IUserService
    {
        Task<bool> ApproveUser(string id);
        Task<bool> BlockUser(string id);
        Task DeleteAsync(string id);
        Task<List<UserDto>> GetAllUser(bool? IsApproved = true);
        Task<PaginatedResult<UserDto>> GetAllUserWithPagination(int pageNumber = 1, int pageSize = 10);
        Task<UserDto?> GetUserByEmail(string email);
        Task<UserDto?> GetUserById(string Id);
    }
}