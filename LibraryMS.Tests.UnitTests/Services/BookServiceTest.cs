using AutoMapper;
using FluentAssertions;
using LibraryMS.Core.Application.Dtos.Book;
using LibraryMS.Core.Application.Dtos.Image;
using LibraryMS.Core.Application.Exceptions;
using LibraryMS.Core.Application.Interfaces;
using LibraryMS.Core.Application.Mappings;
using LibraryMS.Core.Application.Services;
using LibraryMS.Core.Domain.Entities;
using LibraryMS.Infrastructure.Persistence.Contexts;
using LibraryMS.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LibraryMS.Tests.UnitTests.Services
{
    public class BookServiceTest
    {
        private readonly DbContextOptions<LibraryMSContext> _dbContextOptions;
        private readonly Mock<IValidationService> _validationServiceMock;
        private readonly Mock<ICloudinaryService> _cloudinaryServiceMock;
        private readonly IMapper _mapper;

        public BookServiceTest()
        {
            _dbContextOptions = new DbContextOptionsBuilder<LibraryMSContext>()
                .UseInMemoryDatabase($"LibraryMSTestDB_{Guid.NewGuid()}")
                .Options;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<BookMappingProfile>();
                cfg.AddProfile<CategoryMappingProfile>();
            });

            _mapper = config.CreateMapper();
            _validationServiceMock = new Mock<IValidationService>();
            _cloudinaryServiceMock = new Mock<ICloudinaryService>();
        }


        private BookService CreateService()
        {
            var context = new LibraryMSContext(_dbContextOptions);
            var bookRepo = new BookRepository(context);

            return new BookService(
                bookRepo,
                _validationServiceMock.Object,
                _mapper,
                _cloudinaryServiceMock.Object
            );
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
        public async Task GetAllAsync_Should_Return_Only_Available_Books()
        {
            // Arrange
            var service = CreateService();
            var context = new LibraryMSContext(_dbContextOptions);

            Book unavailableBook = CreateBook(Id: 2, title: "Unavailable Book", author: "Author");
            unavailableBook.AvailableCopies = 0;

            context.Books.AddRange(
                CreateBook(Id: 1),
                unavailableBook

            );

            await context.SaveChangesAsync();

            // Act
            var result = await service.GetAllAsync(
                search: null,
                categories: null,
                order: null,
                isAvailable: true,
                page: 1,
                limit: 10
            );

            // Assert
            result.Data.Should().HaveCount(1);
            result.Data.First().Title.Should().Be("Test Title");
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_Search()
        {
            var service = CreateService();
            var context = new LibraryMSContext(_dbContextOptions);

            context.Books.AddRange(
                CreateBook(title: "Clean Code"),
                CreateBook(title: "Domain Driven Design")
            );

            await context.SaveChangesAsync();

            var result = await service.GetAllAsync(
                search: "clean",
                categories: null,
                order: null,
                isAvailable: null,
                page: 1,
                limit: 10
            );

            result.Data.Should().HaveCount(1);
            result.Data.First().Title.Should().Be("Clean Code");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Book_When_Exists()
        {
            var service = CreateService();
            var context = new LibraryMSContext(_dbContextOptions);

            context.Books.Add(CreateBook(Id: 1));
            await context.SaveChangesAsync();

            var result = await service.GetByIdAsync(1);

            result.Should().NotBeNull();
            result!.Title.Should().Be("Test Title");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_Not_Found()
        {
            var service = CreateService();

            var result = await service.GetByIdAsync(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllByCategoryIdAsync_Should_Return_Books_For_Category()
        {
            var service = CreateService();
            var context = new LibraryMSContext(_dbContextOptions);

            var category = new Category { CategoryId = 1, Name = "Programming" };
            var book = CreateBook(Id: 1);
            var bookCategory = new BookCategory { BookId = 1, CategoryId = 1, Category = category };

            book.BookCategories = new List<BookCategory> { bookCategory };

            context.Books.Add(book);
            context.Categories.Add(category);
            context.BookCategories.Add(bookCategory);

            await context.SaveChangesAsync();

            var result = await service.GetAllByCategoryIdAsync(1);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task AddAsync_Should_Create_Book()
        {
            var service = CreateService();

            _validationServiceMock
                .Setup(v => v.ValidateAsync(It.IsAny<AddBookDto>()))
                .Returns(Task.CompletedTask);

            _cloudinaryServiceMock
                .Setup(c => c.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync(new ImageUploadResultDto
                {
                    FileImageUrl = "url",
                    FileImageKey = "key"
                });

            var dto = new AddBookDto
            {
                Title = "New Book",
                Author = "Author",
                CoverFile = null!,
                Description = "Description",
                Summary = "Summary",
                CategoryIds = new List<int>()
            };

            var result = await service.AddAsync(dto);

            result.Should().NotBeNull();
            result!.Title.Should().Be("New Book");
        }

        [Fact]
        public async Task EditAsync_Should_Throw_When_Book_Not_Found()
        {
            var service = CreateService();

            var dto = new EditBookDto { Title = "Updated", Author = "", CategoryIds = new List<int>(), Description = "", Summary = "", PublishDate = DateTime.Now };

            await Assert.ThrowsAsync<ApiException>(() =>
                service.EditAsync(999, dto)
            );
        }

        [Fact]
        public async Task DeleteAsync_Should_Delete_Book()
        {
            var service = CreateService();
            var context = new LibraryMSContext(_dbContextOptions);

            context.Books.Add(CreateBook(Id: 1));
            await context.SaveChangesAsync();

            var result = await service.DeleteAsync(1);

            result.Should().BeTrue();
            context.Books.Count().Should().Be(0);
        }


    }
}
