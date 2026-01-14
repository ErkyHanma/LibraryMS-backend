using AutoMapper;
using LibraryMS_API.Core.Application.Dtos.AccountRequest;
using LibraryMS_API.Core.Application.Dtos.Base;
using LibraryMS_API.Core.Application.Dtos.User;
using LibraryMS_API.Core.Application.Interfaces;
using LibraryMS_API.Core.Domain.Common.Enum;
using LibraryMS_API.Core.Domain.Entities;
using LibraryMS_API.Core.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LibraryMS_API.Core.Application.Services
{
    public class AccountRequestService : IAccountRequestService
    {
        private readonly IAccountRequestRepository _accountRequestRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;


        public AccountRequestService(IAccountRequestRepository accountRequestRepository, IUserService userService, IMapper mapper)
        {
            _accountRequestRepository = accountRequestRepository;
            _userService = userService;
            _mapper = mapper;
        }

        // Get all account requests with pagination, filtering by status, and sorting
        public async Task<PaginatedResult<AccountRequestDto>> GetAllAsync(string? status, string? order = "desc", int page = 1, int limit = 10)
        {

            // Validate parameters
            if (page < 1) page = 1;
            if (limit < 1) limit = 10;
            if (limit > 100) limit = 100;

            var query = _accountRequestRepository.GetAllQuery();

            // Filter by status
            if (!string.IsNullOrEmpty(status))
                switch (status.ToLower())
                {
                    case "pending":
                        query = query.Where(ar => ar.Status == AccountRequestStatus.Pending);
                        break;
                    case "approved":
                        query = query.Where(ar => ar.Status == AccountRequestStatus.Approved);
                        break;
                    case "rejected":
                        query = query.Where(ar => ar.Status == AccountRequestStatus.Rejected);
                        break;
                    default:
                        break;
                }


            // Get total 
            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(total / (double)limit);

            // Apply ordering 
            query = order?.ToLower() == "asc"
                 ? query.OrderBy(c => c.CreatedAt)
                 : query.OrderByDescending(c => c.CreatedAt);

            var items = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();


            var listDtos = new List<AccountRequestDto>();

            foreach (var accountRequest in items)
            {
                // get for each borrow record the user information
                var userDto = await _userService.GetById(accountRequest.UserId);

                var dto = new AccountRequestDto
                {
                    AccountRequestId = accountRequest.AccountRequestId,
                    RejectionReason = accountRequest.RejectionReason,
                    ReviewedAt = accountRequest.ReviewedAt,
                    ReviewedBy = accountRequest.ReviewedBy,
                    CreatedAt = accountRequest.CreatedAt,
                    User = userDto
                };

                listDtos.Add(dto);
            }

            return new PaginatedResult<AccountRequestDto>
            {
                Data = listDtos,
                Meta = new PageMetadata
                {
                    Page = page,
                    Limit = limit,
                    Total = total,
                    TotalPage = totalPages
                }
            };

        }

        public async Task<AccountRequestDto?> GetByIdAsync(int id)
        {
            AccountRequest? entity = await _accountRequestRepository.GetByIdAsync(id);

            if (entity == null)
                return null;

            UserDto? userDto = await _userService.GetById(entity.UserId);

            if (userDto == null)
                return null;

            AccountRequestDto dto = _mapper.Map<AccountRequestDto>(entity);
            dto.User = userDto;

            return dto;
        }

        public async Task<bool> ChangeRequestStatusAsync(int accountRequestId, AccountRequestStatus status, string? rejectionReason)
        {
            return await _accountRequestRepository.ChangeStatus(accountRequestId, status, rejectionReason);
        }
    }
}
