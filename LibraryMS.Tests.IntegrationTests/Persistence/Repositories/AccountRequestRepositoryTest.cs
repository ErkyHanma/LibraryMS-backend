using FluentAssertions;
using LibraryMS.Core.Application.Exceptions;
using LibraryMS.Core.Domain.Common.Enum;
using LibraryMS.Core.Domain.Entities;
using LibraryMS.Infrastructure.Persistence.Contexts;
using LibraryMS.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LibraryMS.Tests.IntegrationTests.Persistence.Repositories
{
    public class AccountRequestRepositoryTest
    {
        private readonly DbContextOptions<LibraryMSContext> _dbContextOptions;

        public AccountRequestRepositoryTest()
        {
            _dbContextOptions = new DbContextOptionsBuilder<LibraryMSContext>()
                .UseInMemoryDatabase(databaseName: $"LibraryMSDb_{Guid.NewGuid()}")
                .Options;
        }

        private static AccountRequest CreateAccountRequest(
            int? id = 0,
            AccountRequestStatus status = AccountRequestStatus.Pending)
        {
            return new AccountRequest
            {
                AccountRequestId = id ?? 0,
                UserId = "test-user-id",
                Status = status
            };
        }

        [Fact]
        public async Task GetAllListAsync_Should_Return_All_AccountRequests()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            context.AccountRequests.AddRange(
                CreateAccountRequest(),
                CreateAccountRequest());
            await context.SaveChangesAsync();

            var repository = new AccountRequestRepository(context);

            // Act
            var result = await repository.GetAllListAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllListAsync_Should_Return_Empty_List_When_No_AccountRequests()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var repository = new AccountRequestRepository(context);

            // Act
            var result = await repository.GetAllListAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void GetAllQuery_Should_Return_IQueryable()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            context.AccountRequests.Add(CreateAccountRequest());
            context.SaveChanges();

            var repository = new AccountRequestRepository(context);

            // Act
            var query = repository.GetAllQuery();

            // Assert
            query.Should().NotBeNull();
            query.Count().Should().Be(1);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_AccountRequest_When_Found()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var request = CreateAccountRequest(1);
            await context.AccountRequests.AddAsync(request);
            await context.SaveChangesAsync();

            var repository = new AccountRequestRepository(context);

            // Act
            var result = await repository.GetByIdAsync(request.AccountRequestId);

            // Assert
            result.Should().NotBeNull();
            result!.AccountRequestId.Should().Be(request.AccountRequestId);
            result.UserId.Should().Be(request.UserId);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_Not_Found()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var repository = new AccountRequestRepository(context);

            // Act
            var result = await repository.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_Should_Add_AccountRequest()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var repository = new AccountRequestRepository(context);
            var request = CreateAccountRequest();

            // Act
            var result = await repository.AddAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.AccountRequestId.Should().Be(request.AccountRequestId);

            var dbRequests = await context.AccountRequests.ToListAsync();
            dbRequests.Should().HaveCount(1);
        }

        [Fact]
        public async Task AddAsync_Should_Throw_When_Entity_Is_Null()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var repository = new AccountRequestRepository(context);

            // Act
            Func<Task> act = async () => await repository.AddAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'entity')");
        }

        [Fact]
        public async Task AddRangeAsync_Should_Add_Multiple_AccountRequests()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var repository = new AccountRequestRepository(context);

            var requests = new List<AccountRequest>
            {
                CreateAccountRequest(),
                CreateAccountRequest()
            };

            // Act
            var result = await repository.AddRangeAsync(requests);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            var dbRequests = await context.AccountRequests.ToListAsync();
            dbRequests.Should().HaveCount(2);
        }

        [Fact]
        public async Task EditAsync_Should_Update_AccountRequest_When_Exists()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            AccountRequest request = CreateAccountRequest(1);
            await context.AccountRequests.AddAsync(request);
            await context.SaveChangesAsync();

            var repository = new AccountRequestRepository(context);

            AccountRequest updated = CreateAccountRequest();
            updated.AccountRequestId = request.AccountRequestId;
            updated.ReviewedBy = "admin-user";

            // Act
            var result = await repository.EditAsync(request.AccountRequestId, updated);

            // Assert
            result.Should().NotBeNull();
            result!.ReviewedBy.Should().Be("admin-user");
        }

        [Fact]
        public async Task EditAsync_Should_Return_Null_When_Not_Found()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var repository = new AccountRequestRepository(context);

            // Act
            var result = await repository.EditAsync(999, CreateAccountRequest());

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_AccountRequest_When_Exists()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var request = CreateAccountRequest(1);
            await context.AccountRequests.AddAsync(request);
            await context.SaveChangesAsync();

            var repository = new AccountRequestRepository(context);

            // Act
            await repository.DeleteAsync(request.AccountRequestId);

            // Assert
            var result = await context.AccountRequests.FindAsync(request.AccountRequestId);
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Not_Throw_When_Not_Found()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var repository = new AccountRequestRepository(context);

            // Act
            Func<Task> act = async () => await repository.DeleteAsync(999);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task ChangeStatus_Should_Approve_Request_When_Pending()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var request = CreateAccountRequest(1);
            await context.AccountRequests.AddAsync(request);
            await context.SaveChangesAsync();

            var repository = new AccountRequestRepository(context);

            // Act
            var result = await repository.ChangeStatus(
                request.AccountRequestId,
                AccountRequestStatus.Approved,
                null, null);

            // Assert
            result.Should().NotBeNull();
            result!.Status.Should().Be(AccountRequestStatus.Approved);
            result.ReviewedAt.Should().NotBeNull();
            result.RejectionReason.Should().BeNull();
        }

        [Fact]
        public async Task ChangeStatus_Should_Reject_Request_When_Pending()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var request = CreateAccountRequest(1);
            await context.AccountRequests.AddAsync(request);
            await context.SaveChangesAsync();

            var repository = new AccountRequestRepository(context);
            var rejectionReason = "Invalid documents";

            // Act
            var result = await repository.ChangeStatus(
                request.AccountRequestId,
                AccountRequestStatus.Rejected,
                null,
                rejectionReason);

            // Assert
            result.Should().NotBeNull();
            result!.Status.Should().Be(AccountRequestStatus.Rejected);
            result.ReviewedAt.Should().NotBeNull();
            result.RejectionReason.Should().Be(rejectionReason);
        }

        [Fact]
        public async Task ChangeStatus_Should_Throw_NotFound_When_Request_Does_Not_Exist()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var repository = new AccountRequestRepository(context);

            // Act
            Func<Task> act = async () =>
                await repository.ChangeStatus(999, AccountRequestStatus.Approved, null, null);

            // Assert
            await act.Should().ThrowAsync<ApiException>()
                .WithMessage("Account request with ID 999 not found.");
        }

        [Fact]
        public async Task ChangeStatus_Should_Throw_When_Request_Is_Not_Pending()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var request = CreateAccountRequest(1, AccountRequestStatus.Approved);
            await context.AccountRequests.AddAsync(request);
            await context.SaveChangesAsync();

            var repository = new AccountRequestRepository(context);

            // Act
            Func<Task> act = async () =>
                await repository.ChangeStatus(
                    request.AccountRequestId,
                    AccountRequestStatus.Rejected,
                    null,
                    "Too late");

            // Assert
            await act.Should().ThrowAsync<ApiException>()
                .WithMessage("Cannot modify account request. This request has already been approved.");
        }

        [Fact]
        public async Task ChangeStatus_Should_Throw_When_Status_Is_Pending()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var request = CreateAccountRequest(1);
            await context.AccountRequests.AddAsync(request);
            await context.SaveChangesAsync();

            var repository = new AccountRequestRepository(context);

            // Act
            Func<Task> act = async () =>
                await repository.ChangeStatus(
                    request.AccountRequestId,
                    AccountRequestStatus.Pending,
                    null,
                    null);

            // Assert
            await act.Should().ThrowAsync<ApiException>()
                .WithMessage("Invalid status transition. Request is already pending.");
        }

        [Fact]
        public async Task ChangeStatus_Should_Throw_When_Invalid_Status_Is_Passed()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var request = CreateAccountRequest(1);
            await context.AccountRequests.AddAsync(request);
            await context.SaveChangesAsync();

            var repository = new AccountRequestRepository(context);

            // Act
            Func<Task> act = async () =>
                await repository.ChangeStatus(
                    request.AccountRequestId,
                    (AccountRequestStatus)999,
                    null,
                    null);

            // Assert
            await act.Should().ThrowAsync<ApiException>()
                .WithMessage("Invalid status: 999. Only 'Approved' or 'Rejected' are allowed.");
        }


    }

}
