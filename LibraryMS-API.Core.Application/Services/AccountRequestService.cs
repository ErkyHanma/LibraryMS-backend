using AutoMapper;
using LibraryMS_API.Core.Application.Dtos.AccountRequest;
using LibraryMS_API.Core.Application.Dtos.Base;
using LibraryMS_API.Core.Application.Dtos.Email;
using LibraryMS_API.Core.Application.Dtos.User;
using LibraryMS_API.Core.Application.Exceptions;
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
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;


        public AccountRequestService(IAccountRequestRepository accountRequestRepository, IUserService userService, IEmailService emailService, IMapper mapper)
        {
            _accountRequestRepository = accountRequestRepository;
            _userService = userService;
            _emailService = emailService;
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
            // Validate if account request exists
            var accountRequest = await _accountRequestRepository.GetByIdAsync(accountRequestId);
            if (accountRequest == null)
                throw ApiException.NotFound($"Account request with ID {accountRequestId} not found.");

            // Validate if the user who made the request exists
            var user = await _userService.GetById(accountRequest.UserId);
            if (user == null)
                throw ApiException.NotFound($"User associated with this request not found.");

            var updatedRequest = await _accountRequestRepository.ChangeStatus(accountRequestId, status, rejectionReason);
            if (updatedRequest == null)
                return false;

            // If request is approved, update user status
            if (status == AccountRequestStatus.Approved)
                await _userService.ChangeStatus(updatedRequest.UserId, UserStatus.Approved);

            // Send confirmation email
            var subject = status == AccountRequestStatus.Approved ? "Account Approved" : "Account Rejected";
            var body = status == AccountRequestStatus.Approved ?
                $@"
                    <h1>LibraryMS</h1>
                    <h2>Congratulations, {user.Name}!</h2>
                    <p>Your account has been approved by the administrator.</p>
                    <p>You can now log in to the LibraryMS using your registered email.</p>
                    <p><strong>University ID:</strong> {user.UniversityId}</p>
                    <p>We look forward to serving you in your academic journey!</p>
                "
                :
                $@"
                <h1>LibraryMS</h1>
                    <h2>Account Request Rejected, {user.Name}</h2>
                    <p>We regret to inform you that your account request has been rejected by the administrator.</p>
                    <p><strong>Reason for Rejection:</strong> {rejectionReason}</p>
                    <p>You can send another request within 15 days</p>
                    <p>If you have any questions or believe this is a mistake, please contact the library administration for further assistance.</p>  
                ";


            await _emailService.SendAsync(new EmailRequestDto
            {
                To = user.Email,
                Subject = subject,
                HtmlBody = body
            });

            return true;
        }
    }
}
