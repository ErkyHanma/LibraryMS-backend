using FluentAssertions;
using LibraryMS.Core.Domain.Entities;
using LibraryMS.Infrastructure.Persistence.Contexts;
using LibraryMS.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LibraryMS.Tests.IntegrationTests.Persistence.Repositories
{
    public class BookRepositoryTest
    {
        private readonly DbContextOptions<LibraryMSContext> _dbContextOptions;

        public BookRepositoryTest()
        {
            _dbContextOptions = new DbContextOptionsBuilder<LibraryMSContext>()
                .UseInMemoryDatabase(databaseName: $"LibraryMSDb_{Guid.NewGuid()}")
                .Options;
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
        public async Task GetAllAsync_Should_Return_All_Books()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            context.Books.AddRange(CreateBook(), CreateBook());
            await context.SaveChangesAsync();
            var repository = new BookRepository(context);

            // Act
            var result = await repository.GetAllListAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_List_When_No_Books()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var repository = new BookRepository(context);

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
            context.Books.Add(CreateBook());
            context.SaveChanges();

            var repository = new BookRepository(context);

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
            Book book = CreateBook(1);
            await context.Books.AddAsync(book);
            var repository = new BookRepository(context);

            // Act
            var result = await repository.GetByIdAsync(book.BookId);

            // Assert
            result.Should().NotBeNull();
            result.BookId.Should().Be(book.BookId);
            result.Title.Should().Be(book.Title);
            result.Author.Should().Be(book.Author);
            result.Description.Should().Be(book.Description);
            result.Summary.Should().Be(book.Summary);
            result.PublishDate.Should().Be(book.PublishDate);
            result.TotalCopies.Should().Be(book.TotalCopies);
            result.AvailableCopies.Should().Be(book.AvailableCopies);
        }

        [Fact]
        public async Task GetById_Should_Return_Null_When_Book_Not_Found()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var repository = new BookRepository(context);

            // Act
            var result = await repository.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_Should_Add_Book_To_Database()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var repository = new BookRepository(context);
            var book = CreateBook();

            // Act
            var result = await repository.AddAsync(book);

            // Assert
            result.Should().NotBeNull();
            result.BookId.Should().Be(book.BookId);
            result.Title.Should().Be(book.Title);
            result.Author.Should().Be(book.Author);
            result.Description.Should().Be(book.Description);
            result.Summary.Should().Be(book.Summary);
            result.PublishDate.Should().Be(book.PublishDate);
            result.TotalCopies.Should().Be(book.TotalCopies);
            result.AvailableCopies.Should().Be(book.AvailableCopies);

            var books = await context.Books.ToListAsync();
            Assert.Single(books);
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
        public async Task AddBookWithCategories_Should_Add_Book_And_Categories()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);

            var categories = new List<Category>
            {
                new() { Name = "Fiction" },
                new() { Name = "Drama" }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();

            var repository = new BookRepository(context);
            var book = CreateBook();
            var categoryIds = categories.Select(c => c.CategoryId).ToList();

            // Act
            var result = await repository.AddBookWithCategories(book, categoryIds);

            // Assert
            result.Should().NotBeNull();
            result!.BookId.Should().BeGreaterThan(0);
            result.BookCategories.Should().HaveCount(2);
        }

        [Fact]
        public async Task AddBookWithCategories_Should_Throw_When_Book_Is_Null()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var repository = new BookRepository(context);

            // Act
            Func<Task> act = async () =>
                await repository.AddBookWithCategories(null!, new List<int>());

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                  .WithMessage("Value cannot be null. (Parameter 'entity')");
        }

        [Fact]
        public async Task AddRangeAsync_Should_Add_Multiple_Categories()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var repository = new BookRepository(context);

            var books = new List<Book>() { CreateBook(), CreateBook() };


            // Act
            var result = await repository.AddRangeAsync(books);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            var dbBooks = await context.Books.ToListAsync();
            dbBooks.Should().HaveCount(2);
        }

        [Fact]
        public async Task EditAsync_Should_Update_Book_When_Exists()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var book = CreateBook(1);
            await context.Books.AddAsync(book);

            var repository = new BookRepository(context);

            var updatedBook = CreateBook(null, "Title updated");

            // Act
            var result = await repository.EditAsync(book.BookId, updatedBook);

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be(updatedBook.Title);
        }

        [Fact]
        public async Task EditAsync_Should_Return_Null_When_Book_Not_Found()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var repository = new BookRepository(context);
            var book = CreateBook();

            // Act
            var result = await repository.EditAsync(999, book);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task EditBookWithCategories_Should_Update_Book_And_Categories()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);

            var categories = new List<Category>
        {
            new() { Name = "Sci-Fi" },
            new() { Name = "Fantasy" },
            new() { Name = "History" }
        };
            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();

            var book = CreateBook(1);
            await context.Books.AddAsync(book);
            await context.SaveChangesAsync();

            var repository = new BookRepository(context);

            var updatedBook = CreateBook(1, "Updated Title");

            var newCategoryIds = categories
                .Take(2)
                .Select(c => c.CategoryId)
                .ToList();

            // Act
            var result = await repository.EditBookWithCategories(
                updatedBook,
                book.BookId,
                newCategoryIds
            );

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be("Updated Title");
            result.BookCategories.Should().HaveCount(2);
        }


        [Fact]
        public async Task EditBookWithCategories_Should_Return_Null_When_Book_Not_Found()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var repository = new BookRepository(context);

            var book = CreateBook();

            // Act
            var result = await repository.EditBookWithCategories(
                book,
                999,
                new List<int>()
            );

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task EditBookWithCategories_Should_Replace_Old_Categories()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);

            var categories = new List<Category>
            {
                new() { Name = "A" },
                new() { Name = "B" },
                new() { Name = "C" }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();

            var book = CreateBook();
            book.BookCategories = new List<BookCategory>
            {
                new() { CategoryId = categories[0].CategoryId },
                new() { CategoryId = categories[1].CategoryId }
            };

            await context.Books.AddAsync(book);
            await context.SaveChangesAsync();

            var repository = new BookRepository(context);

            var newCategories = new List<int> { categories[2].CategoryId };

            // Act
            var result = await repository.EditBookWithCategories(
                book,
                book.BookId,
                newCategories
            );

            // Assert
            result.Should().NotBeNull();
            result!.BookCategories.Should().HaveCount(1);
            result.BookCategories.First().CategoryId.Should().Be(categories[2].CategoryId);
        }


        [Fact]
        public async Task DeleteAsync_Should_Remove_Book_When_Exists()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var book = CreateBook(1);
            await context.Books.AddAsync(book);
            await context.SaveChangesAsync();

            var repository = new BookRepository(context);

            // Act
            await repository.DeleteAsync(book.BookId);

            // Assert
            var result = await context.Books.FindAsync(book.BookId);
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Not_Throw_When_Book_Not_Found()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var repository = new BookRepository(context);

            // Act
            Func<Task> act = async () => await repository.DeleteAsync(999);

            // Assert
            await act.Should().NotThrowAsync();
        }






    }
}
