using FluentAssertions;
using LibraryMS.Core.Application.Interfaces;
using LibraryMS.Core.Application.Services;
using LibraryMS.Core.Domain.Entities;
using LibraryMS.Infrastructure.Persistence.Contexts;
using LibraryMS.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LibraryMS.Tests.UnitTests.Services
{
    public class DashboardServiceTest
    {
        private readonly DbContextOptions<LibraryMSContext> _dbContextOptions;

        public DashboardServiceTest()
        {
            _dbContextOptions = new DbContextOptionsBuilder<LibraryMSContext>()
                .UseInMemoryDatabase($"LibraryMSTestDB_{Guid.NewGuid()}")
                .Options;
        }

        private static BorrowRecord CreateBorrowRecord(int? id = 0)
        {
            return new BorrowRecord
            {
                BorrowRecordId = id ?? 0,
                BookId = 1,
                UserId = "test-user-id",
                BorrowDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(14),

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


        [Fact]
        public async Task GetDashboardStatsAsync_Should_Return_Correct_Statistics()
        {
            // Arrange
            var userServiceMock = new Mock<IUserService>();
            userServiceMock
                .Setup(u => u.GetTotalUserCountAsync())
                .ReturnsAsync(5);

            var context = new LibraryMSContext(_dbContextOptions);

            // Books
            context.Books.AddRange(
                CreateBook(),
                CreateBook(),
                CreateBook()
            );

            // Borrow records
            context.BorrowRecords.AddRange(
                CreateBorrowRecord(),
                new BorrowRecord
                {
                    BookId = 1,
                    UserId = "user-2",
                    BorrowDate = DateTime.UtcNow.AddDays(-20),
                    DueDate = DateTime.UtcNow.AddDays(-5),
                    ReturnDate = null
                },
                new BorrowRecord
                {
                    BookId = 1,
                    UserId = "user-3",
                    BorrowDate = DateTime.UtcNow.AddDays(-30),
                    DueDate = DateTime.UtcNow.AddDays(-10),
                    ReturnDate = DateTime.UtcNow
                }
            );

            await context.SaveChangesAsync();

            var service = new DashboardService(
                new BookRepository(context),
                new BorrowRecordRepository(context),
                userServiceMock.Object
            );

            // Act
            var result = await service.GetDashboardStatsAsync();

            // Assert
            result.Should().NotBeNull();
            result.TotalBooks.Should().Be(3);
            result.TotalBorrowedRecords.Should().Be(2);
            result.TotalOverdueBooks.Should().Be(1);
            result.TotalUsers.Should().Be(5);

            userServiceMock.Verify(
                u => u.GetTotalUserCountAsync(),
                Times.Once);
        }


    }
}
