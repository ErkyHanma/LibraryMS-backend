using AutoMapper;
using FluentAssertions;
using LibraryMS.Core.Application.Dtos.BorrowRecord;
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
    public class BorrowRecordServiceTest
    {
        private readonly DbContextOptions<LibraryMSContext> _dbContextOptions;
        private readonly Mock<IValidationService> _validationServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly IMapper _mapper;

        public BorrowRecordServiceTest()
        {
            _dbContextOptions = new DbContextOptionsBuilder<LibraryMSContext>()
                .UseInMemoryDatabase($"LibraryMSTestDB_{Guid.NewGuid()}")
                .Options;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<BookMappingProfile>();
                cfg.AddProfile<BorrowRecordMappingProfile>();
                cfg.AddProfile<CategoryMappingProfile>();
            });

            _mapper = config.CreateMapper();
            _validationServiceMock = new Mock<IValidationService>();
            _userServiceMock = new Mock<IUserService>();
        }

        public BorrowRecordService CreateService()
        {
            var context = new LibraryMSContext(_dbContextOptions);
            var borrowRecordRepo = new BorrowRecordRepository(context);

            return new BorrowRecordService(
                borrowRecordRepo,
                _userServiceMock.Object,
                _validationServiceMock.Object,
                _mapper
            );
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

        private static Book CreateBook(int? Id = 0, string title = "Test Title", string author = "Test Author")
        {
            return new Book
            {
                BookId = Id ?? 0,
                Title = title,
                Author = author,
                Description = "Description",
                Summary = "Summary",
                Pages = 100,
                PublishDate = DateTime.UtcNow,
                CoverImageUrl = "url",
                CoverImageKey = "key",
                TotalCopies = 10,
                AvailableCopies = 10
            };
        }


        private static BorrowRecord CreateBorrowRecord(
            int id = 1,
            string userId = "user-1",
            int bookId = 1,
            DateTime? returnDate = null,
            DateTime? dueDate = null)
        {
            return new BorrowRecord
            {
                BorrowRecordId = id,
                UserId = userId,
                BookId = bookId,
                BorrowDate = DateTime.UtcNow.AddDays(-2),
                DueDate = dueDate ?? DateTime.UtcNow.AddDays(7),
                ReturnDate = returnDate,
                CreatedAt = DateTime.UtcNow,
            };
        }


        [Fact]
        public async Task GetAllAsync_Should_Return_Paginated_BorrowRecords()
        {
            var service = CreateService();
            var context = new LibraryMSContext(_dbContextOptions);

            context.Books.Add(CreateBook(1));

            context.BorrowRecords.AddRange(
                CreateBorrowRecord(1),
                CreateBorrowRecord(2)
            );

            await context.SaveChangesAsync();

            _userServiceMock
                .Setup(u => u.GetById(It.IsAny<string>()))
                .ReturnsAsync(CreateUserDto());

            var result = await service.GetAllAsync(null, null, null, 1, 10);

            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Meta.Total.Should().Be(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_Overdue_Records()
        {
            var service = CreateService();
            var context = new LibraryMSContext(_dbContextOptions);

            context.Books.Add(CreateBook(1));

            context.BorrowRecords.AddRange(
                CreateBorrowRecord(1, dueDate: DateTime.UtcNow.AddDays(-1)),
                CreateBorrowRecord(2)
            );

            await context.SaveChangesAsync();

            _userServiceMock
                .Setup(u => u.GetById(It.IsAny<string>()))
                .ReturnsAsync(CreateUserDto());

            var result = await service.GetAllAsync(null, "overdue");

            result.Data.Should().HaveCount(1);
        }


        [Fact]
        public async Task GetAllByUserIdAsync_Should_Return_Only_User_Records()
        {
            var service = CreateService();
            var context = new LibraryMSContext(_dbContextOptions);

            context.Books.Add(CreateBook(1));
            context.BorrowRecords.AddRange(
                CreateBorrowRecord(1, userId: "user-1"),
                CreateBorrowRecord(2, userId: "user-2")
            );

            await context.SaveChangesAsync();

            _userServiceMock
                .Setup(u => u.GetById(It.IsAny<string>()))
                .ReturnsAsync(CreateUserDto());

            var result = await service.GetAllByUserIdAsync("user-1", null, null);

            result.Data.Should().HaveCount(1);
            result.Data.First().User!.Id.Should().Be("user-1");
        }


        [Fact]
        public async Task GetById_Should_Return_BorrowRecord_When_Exists()
        {
            var service = CreateService();
            var context = new LibraryMSContext(_dbContextOptions);

            context.Books.Add(CreateBook(1));
            context.BorrowRecords.Add(CreateBorrowRecord(1));
            await context.SaveChangesAsync();

            _userServiceMock
                .Setup(u => u.GetById(It.IsAny<string>()))
                .ReturnsAsync(CreateUserDto());

            var result = await service.GetById(1);

            result.Should().NotBeNull();
            result!.BorrowRecordId.Should().Be(1);
        }

        [Fact]
        public async Task GetById_Should_Return_Null_When_Not_Found()
        {
            var service = CreateService();

            var result = await service.GetById(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task AddBorrowRecordAsync_Should_Create_Record()
        {
            var service = CreateService();
            var context = new LibraryMSContext(_dbContextOptions);


            context.Books.Add(CreateBook(1));
            await context.SaveChangesAsync();

            _validationServiceMock
                .Setup(v => v.ValidateAsync(It.IsAny<AddBorrowRecordDto>()))
                .Returns(Task.CompletedTask);

            _userServiceMock
                .Setup(u => u.GetById("user-1"))
                .ReturnsAsync(CreateUserDto("user-1"));

            var dto = new AddBorrowRecordDto
            {
                UserId = "user-1",
                BookId = 1,
                BorrowDate = DateTime.UtcNow
            };

            var result = await service.AddBorrowRecordAsync(dto);

            result.Should().NotBeNull();
            result!.User!.Id.Should().Be("user-1");
        }


        [Fact]
        public async Task AddBorrowRecordAsync_Should_Throw_When_User_Not_Found()
        {
            var service = CreateService();

            _validationServiceMock
                .Setup(v => v.ValidateAsync(It.IsAny<AddBorrowRecordDto>()))
                .Returns(Task.CompletedTask);

            _userServiceMock
                .Setup(u => u.GetById(It.IsAny<string>()))
                .ReturnsAsync((UserDto?)null);

            var dto = new AddBorrowRecordDto
            {
                UserId = "invalid-user",
                BookId = 1
            };

            await Assert.ThrowsAsync<ApiException>(() =>
                service.AddBorrowRecordAsync(dto)
            );
        }


        [Fact]
        public async Task ReturnBorrowedRecordAsync_Should_Return_True()
        {
            var service = CreateService();
            var context = new LibraryMSContext(_dbContextOptions);
            context.Books.Add(CreateBook(1));
            context.BorrowRecords.Add(CreateBorrowRecord(1));
            await context.SaveChangesAsync();

            var result = await service.ReturnBorrowedRecordAsync(1);

            result.Should().BeTrue();
        }

    }
}
