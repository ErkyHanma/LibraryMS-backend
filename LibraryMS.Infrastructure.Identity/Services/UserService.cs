using LibraryMS.Core.Application.Dtos.Base;
using LibraryMS.Core.Application.Dtos.Image;
using LibraryMS.Core.Application.Dtos.User;
using LibraryMS.Core.Application.Exceptions;
using LibraryMS.Core.Application.Interfaces;
using LibraryMS.Core.Domain.Common.Enum;
using LibraryMS.Core.Domain.Interfaces.Repositories;
using LibraryMS.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace LibraryMS.Infrastructure.Identity.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IBorrowRecordRepository _borrowRecordRepository;

        public UserService(
            UserManager<User> userManager,
            ICloudinaryService cloudinaryService,
            IBorrowRecordRepository borrowRecordRepository)
        {
            _userManager = userManager;
            _cloudinaryService = cloudinaryService;
            _borrowRecordRepository = borrowRecordRepository;
        }

        // Get all users with optional search, sorting, filtering and pagination
        public async Task<PaginatedResult<UserDto>> GetAllAsync(string? search, string? status, string? order = "asc", bool? isApproved = true, int page = 1, int limit = 10)
        {
            // Validate parameters
            if (page < 1) page = 1;
            if (limit < 1) limit = 10;
            if (limit > 100) limit = 100;

            var users = _userManager.Users;

            if (isApproved != null && isApproved == true)
                users = users.Where(u => u.EmailConfirmed && u.Status != UserStatus.Pending);


            // Search users by name, lastname, email or universityId
            if (!string.IsNullOrWhiteSpace(search))
            {
                users = users.Where(u =>
                    u.Name.ToLower().Contains(search.ToLower()) ||
                    u.LastName.ToLower().Contains(search.ToLower()) ||
                    u.Email!.ToLower().Contains(search.ToLower()) ||
                    u.UniversityId.ToLower().Contains(search.ToLower())
                );
            }


            // Get total count for pagination
            var total = await users.CountAsync();
            var totalPages = (int)Math.Ceiling(total / (double)limit);

            // Apply ordering
            users = order?.ToLower() == "asc"
               ? users.OrderBy(c => c.JoinedAt)
               : users.OrderByDescending(c => c.JoinedAt);


            var listUser = await users
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            List<UserDto> listUsersDtos = [];


            foreach (var user in listUser)
            {
                var roleString = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
                if (!Enum.TryParse<Roles>(roleString, out var role))
                {
                    throw new Exception($"Invalid role '{roleString}'");
                }


                listUsersDtos.Add(new UserDto()
                {
                    Id = user.Id,
                    Name = user.Name,
                    LastName = user.LastName,
                    Email = user.Email ?? "",
                    ProfileImageUrl = user.ProfileImageUrl ?? "",
                    UniversityId = user.UniversityId,
                    Status = user.Status,
                    CreatedAt = user.CreatedAt,
                    JoinedAt = user.JoinedAt,
                    UpdatedAt = user.UpdatedAt,
                    Role = role,
                });
            }

            return new PaginatedResult<UserDto>
            {
                Data = listUsersDtos,
                Meta = new PageMetadata
                {
                    Page = page,
                    Limit = limit,
                    Total = total,
                    TotalPage = totalPages
                }
            };
        }

        // Get all users with their borrowed record count. With optional search, sorting, filtering and pagination
        public async Task<PaginatedResult<UserListDto>> GetAllWithBorrowBookAsync(string? search, string? status, string? order = "asc", int page = 1, int limit = 10)
        {
            // Validate parameters
            if (page < 1) page = 1;
            if (limit < 1) limit = 10;
            if (limit > 100) limit = 100;

            var users = _userManager.Users;

            // only users who has been accepted
            users = users.Where(u => u.EmailConfirmed && u.Status != UserStatus.Pending);

            // Search users status
            if (!string.IsNullOrWhiteSpace(status))
            {
                Enum.TryParse<UserStatus>(status, true, out var result);
                users = users.Where(u => u.Status == result);
            }


            // Search users by name, lastname, email or universityId
            if (!string.IsNullOrWhiteSpace(search))
            {
                users = users.Where(u =>
                    u.Name.ToLower().Contains(search.ToLower()) ||
                    u.LastName.ToLower().Contains(search.ToLower()) ||
                    u.Email!.ToLower().Contains(search.ToLower()) ||
                    u.UniversityId.ToLower().Contains(search.ToLower())
                );
            }


            // Get total count for pagination
            var total = await users.CountAsync();
            var totalPages = (int)Math.Ceiling(total / (double)limit);

            // Apply ordering
            users = order?.ToLower() == "asc"
               ? users.OrderBy(c => c.JoinedAt)
               : users.OrderByDescending(c => c.JoinedAt);


            var listUser = await users
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            List<UserListDto> listUsersDtos = [];

            foreach (var item in listUser)
            {
                var rolesList = await _userManager.GetRolesAsync(item);
                var roleString = rolesList.FirstOrDefault();

                if (!Enum.TryParse<Roles>(roleString, out var role))
                {
                    throw new Exception($"Invalid role '{roleString}'");
                }

                var borrowedBooksCount = await _borrowRecordRepository
                    .GetAllQuery().Where(br => br.UserId == item.Id).CountAsync();


                listUsersDtos.Add(new UserListDto()
                {
                    Id = item.Id,
                    Name = item.Name,
                    LastName = item.LastName,
                    Email = item.Email ?? "",
                    ProfileImageUrl = item.ProfileImageUrl ?? "",
                    UniversityId = item.UniversityId,
                    Status = item.Status,
                    CreatedAt = item.CreatedAt,
                    JoinedAt = item.JoinedAt,
                    UpdatedAt = item.UpdatedAt,
                    Role = role,
                    BorrowedBooksCount = borrowedBooksCount
                });
            }

            return new PaginatedResult<UserListDto>
            {
                Data = listUsersDtos,
                Meta = new PageMetadata
                {
                    Page = page,
                    Limit = limit,
                    Total = total,
                    TotalPage = totalPages
                }
            };
        }

        public async Task<List<string>> GetUserIds(string? search)
        {
            var users = _userManager.Users;

            // Search users by name, lastname, email or universityId
            if (!string.IsNullOrWhiteSpace(search))
            {
                users = users.Where(u =>
                    u.Name.ToLower().Contains(search.ToLower()) ||
                    u.LastName.ToLower().Contains(search.ToLower())
                );
            }

            return await users.Select(u => u.Id).ToListAsync();
        }

        public async Task<int> GetTotalUserCountAsync()
        {
            return await _userManager.Users.Where(u => u.Status != UserStatus.Pending).CountAsync();
        }

        public async Task<UserDto?> GetById(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);

            if (user == null)
            {
                return null;
            }

            var rolesList = await _userManager.GetRolesAsync(user);
            var roleString = rolesList.FirstOrDefault();

            if (!Enum.TryParse<Roles>(roleString, out var role))
            {
                throw new Exception($"Invalid role '{roleString}'");
            }

            var userDto = new UserDto()
            {
                Id = user.Id,
                Name = user.Name,
                LastName = user.LastName,
                Email = user.Email ?? "",
                ProfileImageUrl = user.ProfileImageUrl ?? "",
                UniversityId = user.UniversityId,
                Status = user.Status,
                CreatedAt = user.CreatedAt,
                JoinedAt = user.JoinedAt,
                UpdatedAt = user.UpdatedAt,
                Role = role,
            };


            return userDto;
        }

        public async Task<UserDto?> GetByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return null;
            }

            var rolesList = await _userManager.GetRolesAsync(user);
            var roleString = rolesList.FirstOrDefault();

            if (!Enum.TryParse<Roles>(roleString, out var role))
            {
                throw new Exception($"Invalid role '{roleString}'");
            }

            var userDto = new UserDto()
            {
                Id = user.Id,
                Name = user.Name,
                LastName = user.LastName,
                Email = user.Email ?? "",
                ProfileImageUrl = user.ProfileImageUrl ?? "",
                UniversityId = user.UniversityId,
                Status = user.Status,
                CreatedAt = user.CreatedAt,
                JoinedAt = user.JoinedAt,
                UpdatedAt = user.UpdatedAt,
                Role = role,
            };

            return userDto;
        }

        public async Task<UserProfileDto?> GetProfileById(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);

            if (user == null)
                return null;

            var rolesList = await _userManager.GetRolesAsync(user);
            var roleString = rolesList.FirstOrDefault();

            if (!Enum.TryParse<Roles>(roleString, out var role))
            {
                throw new Exception($"Invalid role '{roleString}'");
            }

            var borrowedRecords = _borrowRecordRepository.GetAllQuery().Where(br => br.UserId == user.Id);

            var userDto = new UserProfileDto()
            {
                Id = user.Id,
                Name = user.Name,
                LastName = user.LastName,
                Email = user.Email ?? "",
                ProfileImageUrl = user.ProfileImageUrl ?? "",
                UniversityId = user.UniversityId,
                Status = user.Status,
                JoinedAt = user.JoinedAt,
                Role = role,
                TotalBorrowed = await borrowedRecords.CountAsync(),
                CurrentlyActive = await borrowedRecords.Where(br => br.ReturnDate == null).CountAsync(),
                OverdueBooks = await borrowedRecords
                    .Where(br => br.ReturnDate == null && br.DueDate < DateTime.UtcNow).CountAsync(),
                MaxAllowedBooks = 5
            };


            return userDto;
        }

        public async Task<bool> ChangeRole(string id, Roles role)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return false;

            // Remove existing roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // Add new role
            var result = await _userManager.AddToRoleAsync(user, role.ToString());
            return result.Succeeded;
        }

        public async Task<bool> ChangeStatus(string id, UserStatus status)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return false;

            switch (status)
            {
                case UserStatus.Approved:
                    user.Status = UserStatus.Approved;
                    user.LockoutEnabled = false;
                    user.LockoutEnd = null;

                    if (user.JoinedAt == null)
                        user.JoinedAt = DateTime.UtcNow;
                    break;

                case UserStatus.Blocked:
                    user.Status = UserStatus.Blocked;
                    user.LockoutEnabled = true;
                    user.LockoutEnd = DateTimeOffset.MaxValue;
                    break;
                default:
                    return false;
            }

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<UserDto> EditUserAsync(string id, EditUserDto dto)
        {

            ImageUploadResultDto? newImage = null;
            var existingProfileImageKey = string.Empty;

            try
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user == null)
                    throw ApiException.NotFound($"User with ID {id} not found!");

                var roleList = await _userManager.GetRolesAsync(user);


                if (dto.ProfileImageFile != null)
                {
                    newImage = await _cloudinaryService.UploadImageAsync(dto.ProfileImageFile, "LibraryMS/Users");
                    existingProfileImageKey = user.ProfileImageKey ?? string.Empty;
                }

                user.Name = dto.Name;
                user.LastName = dto.LastName;
                user.ProfileImageUrl = newImage?.FileImageUrl ?? user.ProfileImageUrl;
                user.ProfileImageKey = newImage?.FileImageKey ?? user.ProfileImageKey;

                var result = await _userManager.UpdateAsync(user);

                // Rollback: Delete the newly uploaded image if user update fails
                if (!result.Succeeded && newImage != null)
                    await _cloudinaryService.DeleteImageAsync(newImage.FileImageKey);


                // If new image is uploaded delete old image from cloudinary
                if (!string.IsNullOrEmpty(existingProfileImageKey) && newImage != null)
                {
                    await _cloudinaryService.DeleteImageAsync(existingProfileImageKey);
                }

                return new UserDto()
                {
                    Id = user.Id,
                    Name = user.Name,
                    LastName = user.LastName,
                    Email = user.Email ?? "",
                    ProfileImageUrl = user.ProfileImageUrl ?? "",
                    UniversityId = user.UniversityId,
                    Status = user.Status,
                    CreatedAt = user.CreatedAt,
                    JoinedAt = user.JoinedAt,
                    UpdatedAt = user.UpdatedAt,
                    Role = Enum.Parse<Roles>(roleList.FirstOrDefault() ?? Roles.User.ToString()),
                };

            }
            catch (Exception)
            {
                if (newImage != null)
                    // Rollback: Delete the newly uploaded image if user update fails
                    await _cloudinaryService.DeleteImageAsync(newImage.FileImageKey);

                throw;
            }
        }

        public async Task DeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                throw new ApiException($"There is no account registered with this user ID: {id}", (int)HttpStatusCode.NotFound);

            await _userManager.DeleteAsync(user);
        }

    }
}
