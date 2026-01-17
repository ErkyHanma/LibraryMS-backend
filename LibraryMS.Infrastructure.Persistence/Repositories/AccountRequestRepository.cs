using LibraryMS.Core.Application.Exceptions;
using LibraryMS.Core.Domain.Common.Enum;
using LibraryMS.Core.Domain.Entities;
using LibraryMS.Core.Domain.Interfaces.Repositories;
using LibraryMS.Infrastructure.Persistence.Contexts;
using LibraryMS.Infrastructure.Persistence.Repositories.Base;
using System.Net;

namespace LibraryMS.Infrastructure.Persistence.Repositories
{
    public class AccountRequestRepository : GenericRepository<AccountRequest>, IAccountRequestRepository
    {

        public new readonly LibraryMSContext _context;

        public AccountRequestRepository(LibraryMSContext context) : base(context)
        {
            _context = context;
        }


        public async Task<AccountRequest?> ChangeStatus(int AccountRequestId, AccountRequestStatus status, string? rejectionReason)
        {
            try
            {
                var entity = await _context.Set<AccountRequest>().FindAsync(AccountRequestId);
                if (entity == null)
                    throw ApiException.NotFound($"Account request with ID {AccountRequestId} not found.");

                // Only update if the current status is pending
                if (entity.Status != AccountRequestStatus.Pending)
                    throw ApiException.BadRequest($"Cannot modify account request. This request has already been {entity.Status.ToString().ToLower()}.");

                // Validate the new status
                if (status == AccountRequestStatus.Pending)
                    throw ApiException.BadRequest("Invalid status transition. Request is already pending.");

                switch (status)
                {
                    case AccountRequestStatus.Approved:
                        entity.Status = AccountRequestStatus.Approved;
                        entity.ReviewedAt = DateTime.UtcNow;
                        entity.RejectionReason = rejectionReason;
                        break;
                    case AccountRequestStatus.Rejected:
                        entity.Status = AccountRequestStatus.Rejected;
                        entity.ReviewedAt = DateTime.UtcNow;
                        entity.RejectionReason = rejectionReason;

                        break;
                    default:
                        throw ApiException.BadRequest($"Invalid status: {status}. Only 'Approved' or 'Rejected' are allowed.");
                }

                await _context.SaveChangesAsync();
                return entity;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApiException(
                    $"An unexpected error occurred while updating account request status: {ex.Message}",
                    (int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
