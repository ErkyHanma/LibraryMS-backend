using LibraryMS_API.Core.Application.Dtos.Base;
using LibraryMS_API.Core.Application.Dtos.User;
using LibraryMS_API.Core.Application.Exceptions;
using LibraryMS_API.Core.Application.Interfaces;
using LibraryMS_API.Core.Domain.Common.Enum;
using LibraryMS_API.Core.Domain.Interfaces.Repositories;
using LibraryMS_API.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace LibraryMS_API.Infrastructure.Identity.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IBorrowRecordRepository _borrowRecordRepository;

        public UserService(
            UserManager<User> userManager,
            IBorrowRecordRepository borrowRecordRepository)
        {
            _userManager = userManager;
            _borrowRecordRepository = borrowRecordRepository;
        }

        // Get all users with optional search, sorting, filtering and pagination
        public async Task<PaginatedResult<UserDto>> GetAllAsync(string? search, string? order = "asc", bool? isApproved = true, int page = 1, int limit = 10)
        {
            // Validate parameters
            if (page < 1) page = 1;
            if (limit < 1) limit = 10;
            if (limit > 100) limit = 100;

            var users = _userManager.Users;

            if (isApproved != null && isApproved == true)
                users = users.Where(u => u.EmailConfirmed && u.Status != UserStatus.Pending);


            // Search users by FullName, Email, UniversityId
            if (!string.IsNullOrWhiteSpace(search))
            {
                users = users.Where(u =>
                    u.FullName.ToLower().Contains(search.ToLower()) ||
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
                    FullName = user.FullName,
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
        public async Task<PaginatedResult<UserListDto>> GetAllWithBorrowBookAsync(string? search, string? order = "asc", bool? isApproved = true, int page = 1, int limit = 10)
        {
            // Validate parameters
            if (page < 1) page = 1;
            if (limit < 1) limit = 10;
            if (limit > 100) limit = 100;

            var users = _userManager.Users;

            if (isApproved != null && isApproved == true)
                users = users.Where(u => u.EmailConfirmed && u.Status != UserStatus.Pending);


            // Search users by FullName, Email, UniversityId
            if (!string.IsNullOrWhiteSpace(search))
            {
                users = users.Where(u =>
                    u.FullName.ToLower().Contains(search.ToLower()) ||
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
                    FullName = item.FullName,
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
                FullName = user.FullName,
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
                FullName = user.FullName,
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
                FullName = user.FullName,
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

        public async Task<bool> ApproveUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                if (user.Status == UserStatus.Pending)
                {
                    user.Status = UserStatus.Approved;
                    user.LockoutEnabled = false;
                    user.LockoutEnd = null;
                }

                await _userManager.UpdateAsync(user);
                return true;
            }

            return false;
        }

        public async Task<bool> BlockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                user.Status = UserStatus.Blocked;
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue;

                await _userManager.UpdateAsync(user);
                return true;
            }

            return false;
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

        //public virtual async Task<EditUserResponseDto> EditUserAsync(EditUserDto dto, string? origin, bool? isCreated = false, bool? isApi = false)
        //{
        //    bool isNotcreated = !isCreated ?? false;
        //    EditUserResponseDto response = new()
        //    {
        //        Email = "",
        //        Id = "",
        //        LastName = "",
        //        Name = "",
        //        UserName = "",
        //        IdentificationNumber = "",
        //        Role = "",
        //        HasError = false,
        //        Errors = []
        //    };

        //    var userWithSameUserName = await _userManager.Users.FirstOrDefaultAsync(w => w.UserName == dto.UserName && w.Id != dto.Id);
        //    if (userWithSameUserName != null)
        //    {
        //        response.HasError = true;
        //        response.Errors.Add($"this username: {dto.UserName} is already taken.");
        //        return response;
        //    }

        //    var userWithSameEmail = await _userManager.Users.FirstOrDefaultAsync(w => w.Email == dto.Email && w.Id != dto.Id);
        //    if (userWithSameEmail != null)
        //    {
        //        response.HasError = true;
        //        response.Errors.Add($"this email: {dto.Email} is already taken.");
        //        return response;
        //    }

        //    var userWithSameIdentificationNumber = await _userManager.Users
        //        .FirstOrDefaultAsync(w => w.IdentificationNumber == dto.IdentificationNumber && w.Id != dto.Id);

        //    if (userWithSameIdentificationNumber != null)
        //    {
        //        response.HasError = true;
        //        response.Errors.Add($"this identification number: {dto.IdentificationNumber} is already taken.");
        //        return response;
        //    }

        //    var user = await _userManager.FindByIdAsync(dto.Id);
        //    var roleList = await _userManager.GetRolesAsync(user);

        //    if (user == null)
        //    {
        //        response.HasError = true;
        //        response.Errors.Add($"There is no account registered with this user");
        //        return response;
        //    }

        //    user.Name = dto.Name;
        //    user.LastName = dto.LastName;
        //    user.UserName = dto.UserName;
        //    user.IdentificationNumber = dto.IdentificationNumber;
        //    user.EmailConfirmed = user.EmailConfirmed && user.Email == dto.Email;
        //    user.Email = dto.Email;

        //    if (!string.IsNullOrWhiteSpace(dto.Password) && isNotcreated)
        //    {
        //        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        //        var resultChange = await _userManager.ResetPasswordAsync(user, token, dto.Password);

        //        if (resultChange != null && !resultChange.Succeeded)
        //        {
        //            response.HasError = true;
        //            response.Errors.AddRange(resultChange.Errors.Select(s => s.Description).ToList());
        //            return response;
        //        }
        //    }


        //    // edit initial balance just for customer users
        //    if (roleList.FirstOrDefault() == Roles.Customer.ToString())
        //    {
        //        var userMainAccount = await _savingAccountRepository.GetMainAccountByUserIdAsync(user.Id);

        //        if (userMainAccount == null)
        //        {
        //            response.HasError = true;
        //            response.Errors.Add("Either account or user don't exists. Please try again.");
        //            return response;
        //        }

        //        userMainAccount.CurrentBalance += dto.InitialBalance ?? 0;
        //        await _savingAccountRepository.UpdateAsync(userMainAccount.SavingAccountId, userMainAccount);
        //    }



        //    var result = await _userManager.UpdateAsync(user);

        //    if (result.Succeeded)
        //    {

        //        if (!user.EmailConfirmed && isNotcreated)
        //        {
        //            if (isApi != null && !isApi.Value)
        //            {
        //                string verificationUri = await GetVerificationEmailUri(user, origin ?? "");
        //                await _emailService.SendAsync(new EmailRequestDto()
        //                {
        //                    To = dto.Email,
        //                    HtmlBody = $"Please confirm your account visiting this URL {verificationUri}",
        //                    Subject = "Confirm registration"
        //                });
        //            }
        //            else
        //            {
        //                string? verificationToken = await GetVerificationEmailToken(user);
        //                await _emailService.SendAsync(new EmailRequestDto()
        //                {
        //                    To = dto.Email,
        //                    HtmlBody = $"Please confirm your account use this token {verificationToken}",
        //                    Subject = "Confirm registration"
        //                });
        //            }
        //        }




        //        response.Id = user.Id;
        //        response.Email = user.Email ?? "";
        //        response.UserName = user.UserName ?? "";
        //        response.Name = user.Name;
        //        response.LastName = user.LastName;
        //        response.IsActive = user.EmailConfirmed;
        //        response.Role = roleList.FirstOrDefault() ?? "";

        //        return response;
        //    }
        //    else
        //    {
        //        response.HasError = true;
        //        response.Errors.AddRange(result.Errors.Select(s => s.Description).ToList());
        //        return response;
        //    }
        //}

        public async Task DeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                throw new ApiException($"There is no account registered with this user ID: {id}", (int)HttpStatusCode.NotFound);

            await _userManager.DeleteAsync(user);
        }

    }
}
