using FluentAssertions;
using LibraryMS.Core.Application.Exceptions;
using LibraryMS.Core.Domain.Entities;
using LibraryMS.Infrastructure.Persistence.Contexts;
using LibraryMS.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LibraryMS.Tests.IntegrationTests.Persistence.Repositories
{
    public class BorrowRecordRepositoryTest
    {
        private readonly DbContextOptions<LibraryMSContext> _dbContextOptions;

        public BorrowRecordRepositoryTest()
        {
            _dbContextOptions = new DbContextOptionsBuilder<LibraryMSContext>()
                .UseInMemoryDatabase(databaseName: $"LibraryMSDb_{Guid.NewGuid()}")
                .Options;
        }

        private static BorrowRecord CreateBorrowRecord(int? id = 0)
        {
            return new BorrowRecord
            {
                BorrowRecordId = id ?? 0,
                BookId = 1,
                UserId = "test-user-id",

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
        public async Task GetAllAsync_Should_Return_All_BorrowRecord()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            context.BorrowRecords.AddRange(CreateBorrowRecord(), CreateBorrowRecord());
            await context.SaveChangesAsync();
            var repository = new BorrowRecordRepository(context);

            // Act
            var result = await repository.GetAllListAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_List_When_No_BorrowRecord()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var repository = new BorrowRecordRepository(context);

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
            context.BorrowRecords.Add(CreateBorrowRecord());
            context.SaveChanges();

            var repository = new BorrowRecordRepository(context);

            // Act
            var query = repository.GetAllQuery();

            // Assert
            query.Should().NotBeNull();
            query.Count().Should().Be(1);
        }

        [Fact]
        public async Task GetById_Should_Return_Book_By_Id()
        {
            // Arrnge
            using var context = new LibraryMSContext(_dbContextOptions);
            BorrowRecord borrowRecord = CreateBorrowRecord(1);
            await context.BorrowRecords.AddAsync(borrowRecord);
            var repository = new BorrowRecordRepository(context);

            // Act
            var result = await repository.GetByIdAsync(borrowRecord.BorrowRecordId);

            // Assert
            result.Should().NotBeNull();
            result.BorrowRecordId.Should().Be(borrowRecord.BorrowRecordId);
            result.BookId.Should().Be(borrowRecord.BookId);
            result.UserId.Should().Be(borrowRecord.UserId);
        }

        [Fact]
        public async Task GetById_Should_Return_Null_When_BorrowRecord_Not_Found()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var repository = new BorrowRecordRepository(context);

            // Act
            var result = await repository.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_Should_Add_BorrowRecord_To_Database()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);

            var book = CreateBook();
            await context.Books.AddAsync(book);

            var repository = new BorrowRecordRepository(context);
            var borrowRecord = CreateBorrowRecord();

            // Act
            var result = await repository.AddAsync(borrowRecord);

            // Assert
            result.Should().NotBeNull();
            result.BorrowRecordId.Should().Be(borrowRecord.BorrowRecordId);
            result.BookId.Should().Be(borrowRecord.BookId);
            result.UserId.Should().Be(borrowRecord.UserId);


            var borrowRecords = await context.BorrowRecords.ToListAsync();
            Assert.Single(borrowRecords);
        }

        [Fact]
        public async Task AddAsync_Should_Throw_Exception_When_Null()
        {
            using var context = new LibraryMSContext(_dbContextOptions);
            var repository = new BookRepository(context);

            // Act
            Func<Task> act = async () => await repository.AddAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                        .WithMessage("Value cannot be null. (Parameter 'entity')");
        }

        [Fact]
        public async Task AddAsync_Should_Throw_Exception_When_Book_Not_Found()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);

            var repository = new BorrowRecordRepository(context);
            var borrowRecord = CreateBorrowRecord();

            // Act
            Func<Task> act = async () => await repository.AddAsync(borrowRecord);


            // Assert
            await act.Should().ThrowAsync<ApiException>()
                        .WithMessage($"Book with ID {borrowRecord.BookId} not found");
        }

        [Fact]
        public async Task AddAsync_Should_Throw_Exception_When_Book_Not_Have_Available_Copies()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);

            var book = CreateBook();
            await context.Books.AddAsync(book);
            book.AvailableCopies = 0;

            var repository = new BorrowRecordRepository(context);
            var borrowRecord = CreateBorrowRecord();
            borrowRecord.BookId = book.BookId;

            // Act
            Func<Task> act = async () => await repository.AddAsync(borrowRecord);


            // Assert
            await act.Should().ThrowAsync<ApiException>()
                        .WithMessage($"Cannot borrow book '{book.Title}'. No copies available");
        }

        [Fact]
        public async Task AddRangeAsync_Should_Add_Multiple_BorrowRecord()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var repository = new BorrowRecordRepository(context);

            var borrowRecords = new List<BorrowRecord>() { CreateBorrowRecord(), CreateBorrowRecord() };


            // Act
            var result = await repository.AddRangeAsync(borrowRecords);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            var dbBorrowRecords = await context.BorrowRecords.ToListAsync();
            dbBorrowRecords.Should().HaveCount(2);
        }

        [Fact]
        public async Task EditAsync_Should_Update_Book_When_Exists()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var borrowRecord = CreateBorrowRecord(1);
            await context.BorrowRecords.AddAsync(borrowRecord);

            var repository = new BorrowRecordRepository(context);

            var updatedBook = CreateBorrowRecord();
            updatedBook.ReturnDate = DateTime.Now;

            // Act
            var result = await repository.EditAsync(borrowRecord.BorrowRecordId, updatedBook);

            // Assert
            result.Should().NotBeNull();
            result!.ReturnDate.Should().Be(updatedBook.ReturnDate);
        }

        [Fact]
        public async Task EditAsync_Should_Return_Null_When_BorrowRecord_Not_Found()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var repository = new BorrowRecordRepository(context);
            var book = CreateBorrowRecord();

            // Act
            var result = await repository.EditAsync(999, book);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_BorrowRecord_When_Exists()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var borrowRecord = CreateBorrowRecord(1);
            await context.BorrowRecords.AddAsync(borrowRecord);
            await context.SaveChangesAsync();

            var repository = new BorrowRecordRepository(context);

            // Act
            await repository.DeleteAsync(borrowRecord.BorrowRecordId);

            // Assert
            var result = await context.BorrowRecords.FindAsync(borrowRecord.BorrowRecordId);
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Not_Throw_When_BorrowRecord_Not_Found()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var repository = new BorrowRecordRepository(context);

            // Act
            Func<Task> act = async () => await repository.DeleteAsync(999);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task ReturnBorrowedRecordAsync_Should_Return_True_When_Successful()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var book = CreateBook();
            await context.Books.AddAsync(book);
            var borrowRecord = CreateBorrowRecord();
            await context.BorrowRecords.AddAsync(borrowRecord);
            var repository = new BorrowRecordRepository(context);

            // Act
            var result = await repository.ReturnBorrowedRecordAsync(borrowRecord.BorrowRecordId);

            // Assert
            result.Should().BeTrue();
            var updatedBorrowRecord = await context.BorrowRecords.FindAsync(borrowRecord.BorrowRecordId);
            updatedBorrowRecord!.ReturnDate.Should().NotBeNull();
            var updatedBook = await context.Books.FindAsync(book.BookId);
            updatedBook!.AvailableCopies.Should().Be(book.AvailableCopies);
        }

        [Fact]
        public async Task ReturnBorrowedRecordAsync_Should_Throw_Exception_When_BorrowRecord_Not_Found()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var repository = new BorrowRecordRepository(context);
            var borrowedRecordId = 999;
            // Act
            Func<Task> act = async () => await repository.ReturnBorrowedRecordAsync(borrowedRecordId);

            // Assert
            await act.Should().ThrowAsync<ApiException>()
                        .WithMessage($"Borrowed record with ID {borrowedRecordId} not found");
        }

        [Fact]
        public async Task ReturnBorrowedRecordAsync_Should_Throw_Exception_When_BorrowRecord_Is_Already_Returned()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var borrowRecord = CreateBorrowRecord(1);
            borrowRecord.ReturnDate = DateTime.Now;
            await context.BorrowRecords.AddAsync(borrowRecord);
            var repository = new BorrowRecordRepository(context);

            // Act
            Func<Task> act = async () => await repository.ReturnBorrowedRecordAsync(borrowRecord.BorrowRecordId);

            // Assert
            await act.Should().ThrowAsync<ApiException>()
                        .WithMessage($"This borrowed record is already returned.");
        }

        [Fact]
        public async Task ReturnBorrowedRecordAsync_Should_Throw_Exception_When_Book_Not_Found()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var borrowRecord = CreateBorrowRecord(1);
            await context.BorrowRecords.AddAsync(borrowRecord);

            var repository = new BorrowRecordRepository(context);

            // Act
            Func<Task> act = async () => await repository.ReturnBorrowedRecordAsync(borrowRecord.BorrowRecordId);

            // Assert
            await act.Should().ThrowAsync<ApiException>()
                        .WithMessage($"Borrowed Book not found");
        }


    }
}
