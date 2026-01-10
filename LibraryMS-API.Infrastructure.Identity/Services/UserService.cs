using LibraryMS_API.Core.Application.Dtos.Base;
using LibraryMS_API.Core.Application.Dtos.User;
using LibraryMS_API.Core.Application.Exceptions;
using LibraryMS_API.Core.Application.Interfaces;
using LibraryMS_API.Core.Domain.Common.Enum;
using LibraryMS_API.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace LibraryMS_API.Infrastructure.Identity.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;

        public UserService(
            UserManager<User> userManager)
        {
            _userManager = userManager;
        }


        public async Task<List<UserDto>> GetAllUser(bool? IsApproved = true)
        {
            List<UserDto> listUsersDtos = [];

            var users = _userManager.Users;

            if (IsApproved != null && IsApproved == true)
            {
                users = users.Where(u => u.EmailConfirmed && u.Status != UserStatus.Pending);
            }

            var listUser = await users.ToListAsync();

            foreach (var item in listUser)
            {
                var rolesList = await _userManager.GetRolesAsync(item);
                var roleString = rolesList.FirstOrDefault();

                if (!Enum.TryParse<Roles>(roleString, out var role))
                {
                    throw new Exception($"Invalid role '{roleString}'");
                }


                listUsersDtos.Add(new UserDto()
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
                });
            }

            return listUsersDtos;
        }

        public IQueryable<User> GetAllUserQuery()
        {
            return _userManager.Users;
        }

        //public virtual async Task<PaginatedResult<UserDto>> GetAllUserWithPagination(int pageNumber = 1, int pageSize = 10)
        //{
        //    List<UserDto> listUsersDtos = new();
        //    var skipNumber = (pageNumber - 1) * pageSize;

        //    int totalCount;
        //    IEnumerable<User> pagedUsers;


        //    var query = _userManager.Users.OrderByDescending(u => u.CreatedAt);
        //    totalCount = await query.CountAsync();
        //    pagedUsers = await query.Skip(skipNumber).Take(pageSize).ToListAsync();

        //    int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        //    foreach (var user in pagedUsers)
        //    {
        //        var rolesList = await _userManager.GetRolesAsync(user);
        //        var roleString = rolesList.FirstOrDefault();

        //        if (!Enum.TryParse<Roles>(roleString, out var role))
        //        {
        //            throw new Exception($"Invalid role '{roleString}'");
        //        }

        //        listUsersDtos.Add(new UserDto()
        //        {
        //            Id = user.Id,
        //            FullName = user.FullName,
        //            Email = user.Email ?? "",
        //            ProfileImageUrl = user.ProfileImageUrl ?? "",
        //            UniversityId = user.UniversityId,
        //            Status = user.Status,
        //            CreatedAt = user.CreatedAt,
        //            JoinedAt = user.JoinedAt,
        //            UpdatedAt = user.UpdatedAt,
        //            Role = role,
        //        });

        //    }


        //    var result = new PaginatedResult<UserDto>()
        //    {
        //        Items = listUsersDtos,
        //        TotalPages = totalPages,
        //        CurrentPage = pageNumber,
        //    };

        //    return result;
        //}

        public async Task<UserDto?> GetUserById(string Id)
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

        public async Task<UserDto?> GetUserByEmail(string email)
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

        public virtual async Task DeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                throw new ApiException($"There is no account registered with this user ID: {id}", (int)HttpStatusCode.NotFound);

            await _userManager.DeleteAsync(user);
        }

        public Task<PaginatedResult<UserDto>> GetAllUserWithPagination(int pageNumber = 1, int pageSize = 10)
        {
            throw new NotImplementedException();
        }
    }
}
