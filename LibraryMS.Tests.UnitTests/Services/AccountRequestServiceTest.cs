using AutoMapper;
using FluentAssertions;
using LibraryMS.Core.Application.Dtos.Email;
using LibraryMS.Core.Application.Dtos.User;
using LibraryMS.Core.Application.Exceptions;
using LibraryMS.Core.Application.Interfaces;
using LibraryMS.Core.Application.Mappings;
using LibraryMS.Core.Application.Services;
using LibraryMS.Core.Domain.Common.Enum;
using LibraryMS.Core.Domain.Entities;
using LibraryMS.Infrastructure.Persistence.Contexts;
using LibraryMS.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LibraryMS.Tests.UnitTests.Services
{
    public class AccountRequestServiceTest
    {
        private readonly DbContextOptions<LibraryMSContext> _dbContextOptions;
        private readonly Mock<IEmailService> _emailService;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly IMapper _mapper;

        public AccountRequestServiceTest()
        {
            _dbContextOptions = new DbContextOptionsBuilder<LibraryMSContext>()
                .UseInMemoryDatabase($"LibraryMSTestDB_{Guid.NewGuid()}")
                .Options;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AccountRequestMappingProfile>();
            });

            _mapper = config.CreateMapper();
            _emailService = new Mock<IEmailService>();
            _userServiceMock = new Mock<IUserService>();
        }



        public AccountRequestService CreateService()
        {
            var context = new LibraryMSContext(_dbContextOptions);
            var accountRequestRepo = new AccountRequestRepository(context);

            return new AccountRequestService(
                accountRequestRepo,
                _userServiceMock.Object,
                _emailService.Object,
                _mapper
            );
        }


        private static AccountRequest CreateAccountRequest(
        int id = 0,
        string userId = "user-1",
        AccountRequestStatus status = AccountRequestStatus.Pending)
        {
            return new AccountRequest
            {
                AccountRequestId = id,
                UserId = userId,
                Status = status,
                CreatedAt = DateTime.UtcNow
            };
        }

        private static UserDto CreateUserDto(string id = "user-1")
        {
            return new UserDto
            {
                Id = id,
                Name = "Test",
                LastName = "User",
                Email = "",
                ProfileImageUrl = "",
                Role = Roles.User,
                UniversityId = "",
                Status = UserStatus.Approved,
            };
        }


        [Fact]
        public async Task GetAllAsync_Should_Return_Paginated_AccountRequests()
        {
            // Arrange
            var service = CreateService();
            var context = new LibraryMSContext(_dbContextOptions);

            context.AccountRequests.AddRange(
                CreateAccountRequest(1),
                CreateAccountRequest(2)
            );
            await context.SaveChangesAsync();

            _userServiceMock
                .Setup(x => x.GetById(It.IsAny<string>()))
                .ReturnsAsync(CreateUserDto());

            // Act
            var result = await service.GetAllAsync(null);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Meta.Total.Should().Be(2);
        }


        [Fact]
        public async Task GetAllAsync_Should_Filter_By_Status()
        {
            // Arrange
            var service = CreateService();
            var context = new LibraryMSContext(_dbContextOptions);

            context.AccountRequests.AddRange(
                CreateAccountRequest(1, status: AccountRequestStatus.Pending),
                CreateAccountRequest(2, status: AccountRequestStatus.Approved)
            );
            await context.SaveChangesAsync();

            _userServiceMock
                .Setup(x => x.GetById(It.IsAny<string>()))
                .ReturnsAsync(CreateUserDto());

            // Act
            var result = await service.GetAllAsync("approved");

            // Assert
            result.Data.Should().HaveCount(1);
        }


        [Fact]
        public async Task GetByIdAsync_Should_Return_AccountRequestDto_When_Exists()
        {
            // Arrange
            var service = CreateService();
            var context = new LibraryMSContext(_dbContextOptions);

            var entity = CreateAccountRequest(1);
            context.AccountRequests.Add(entity);
            await context.SaveChangesAsync();

            _userServiceMock
                .Setup(x => x.GetById(entity.UserId))
                .ReturnsAsync(CreateUserDto(entity.UserId));

            // Act
            var result = await service.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.AccountRequestId.Should().Be(1);
        }


        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_Not_Found()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }


        [Fact]
        public async Task ChangeRequestStatusAsync_Should_Approve_Request_And_Send_Email()
        {
            // Arrange
            var service = CreateService();
            var context = new LibraryMSContext(_dbContextOptions);

            var request = CreateAccountRequest(1);
            context.AccountRequests.Add(request);
            await context.SaveChangesAsync();

            var user = CreateUserDto(request.UserId);

            _userServiceMock.Setup(x => x.GetById(request.UserId))
                .ReturnsAsync(user);

            //_userServiceMock.Setup(x =>
            //    x.ChangeStatus(request.UserId, UserStatus.Approved))
            //    .Returns(Task.CompletedTask);

            _emailService.Setup(x =>
                x.SendAsync(It.IsAny<EmailRequestDto>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await service.ChangeRequestStatusAsync(
                request.AccountRequestId,
                AccountRequestStatus.Approved,
                null);

            // Assert
            result.Should().BeTrue();

            _emailService.Verify(
                x => x.SendAsync(It.IsAny<EmailRequestDto>()),
                Times.Once);

            _userServiceMock.Verify(
                x => x.ChangeStatus(request.UserId, UserStatus.Approved),
                Times.Once);
        }


        [Fact]
        public async Task ChangeRequestStatusAsync_Should_Reject_Request_And_Send_Email()
        {
            // Arrange
            var service = CreateService();
            var context = new LibraryMSContext(_dbContextOptions);

            var request = CreateAccountRequest(1);
            context.AccountRequests.Add(request);
            await context.SaveChangesAsync();

            _userServiceMock.Setup(x => x.GetById(request.UserId))
                .ReturnsAsync(CreateUserDto());

            _emailService.Setup(x =>
                x.SendAsync(It.IsAny<EmailRequestDto>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await service.ChangeRequestStatusAsync(
                request.AccountRequestId,
                AccountRequestStatus.Rejected,
                "Invalid data");

            // Assert
            result.Should().BeTrue();
        }


        [Fact]
        public async Task ChangeRequestStatusAsync_Should_Throw_NotFound_When_Request_Not_Exists()
        {
            // Arrange
            var service = CreateService();

            // Act
            Func<Task> act = () =>
                service.ChangeRequestStatusAsync(999, AccountRequestStatus.Approved, null);

            // Assert
            await act.Should().ThrowAsync<ApiException>()
                .WithMessage("*not found*");
        }


    }
}
