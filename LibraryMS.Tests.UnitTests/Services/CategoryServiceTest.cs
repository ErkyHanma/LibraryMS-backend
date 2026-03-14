using AutoMapper;
using FluentAssertions;
using FluentValidation.Results;
using LibraryMS.Core.Application.Dtos.Category;
using LibraryMS.Core.Application.Interfaces;
using LibraryMS.Core.Application.Mappings;
using LibraryMS.Core.Application.Services;
using LibraryMS.Core.Domain.Entities;
using LibraryMS.Infrastructure.Persistence.Contexts;
using LibraryMS.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LibraryMS.Tests.UnitTests.Services
{
    public class CategoryServiceTest
    {
        private readonly DbContextOptions<LibraryMSContext> _dbContextOptions;
        private readonly Mock<IValidationService> _validationServiceMock;
        private readonly IMapper _mapper;

        public CategoryServiceTest()
        {
            _dbContextOptions = new DbContextOptionsBuilder<LibraryMSContext>()
                .UseInMemoryDatabase($"LibraryMSTestDB_{Guid.NewGuid()}")
                .Options;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CategoryMappingProfile>();
            });

            _mapper = config.CreateMapper();
            _validationServiceMock = new Mock<IValidationService>();
        }

        private CategoryService CreateService()
        {
            var context = new LibraryMSContext(_dbContextOptions);
            var categoryRepo = new CategoryRepository(context);

            return new CategoryService(
                categoryRepo,
                _validationServiceMock.Object,
                _mapper
            );
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_CategoryDto_List()
        {
            //Arrange
            var service = CreateService();
            var context = new LibraryMSContext(_dbContextOptions);

            var categoryList = new List<Category>()
            {
                new() { Name = "Fiction" } ,
                new() { Name = "Sci-Fi" } ,
            };

            context.Categories.AddRange(categoryList);
            await context.SaveChangesAsync();
            //Act
            var result = await service.GetAllAsync();

            //Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().HaveCount(categoryList.Count);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_List_When_No_Categories()
        {
            //Arrange
            var service = CreateService();

            //Act
            var result = await service.GetAllAsync();

            //Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }


        [Fact]
        public async Task GetByIdAsync_Should_Return_CategoryDto()
        {
            //Arrange
            var service = CreateService();
            var context = new LibraryMSContext(_dbContextOptions);
            Category category = new() { Name = "Fiction" };

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            //Act
            var result = await service.GetByIdAsync(category.CategoryId);

            //Assert
            result.Should().NotBeNull();
            result.CategoryId.Should().Be(category.CategoryId);
            result.Name.Should().Be(category.Name);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_Category_Not_Found()
        {
            //Arrange
            var service = CreateService();
            var context = new LibraryMSContext(_dbContextOptions);

            //Act
            var result = await service.GetByIdAsync(999);

            //Assert
            result.Should().BeNull();
        }


        [Fact]
        public async Task AddAsync_Should_Add_Category_And_Return_Dto_When_Valid()
        {
            // Arrange
            var service = CreateService();
            var dto = new AddCategoryDto
            {
                Name = "Fantasy"
            };

            _validationServiceMock
                .Setup(v => v.ValidateAsync(dto))
                .Returns(Task.CompletedTask);

            // Act
            var result = await service.AddAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result!.CategoryId.Should().BeGreaterThan(0);
            result.Name.Should().Be(dto.Name);

            _validationServiceMock.Verify(
                v => v.ValidateAsync(dto),
                Times.Once);
        }

        [Fact]
        public async Task AddAsync_Should_Throw_When_Validation_Fails()
        {
            // Arrange
            var service = CreateService();
            var dto = new AddCategoryDto { Name = "" };

            _validationServiceMock
                .Setup(v => v.ValidateAsync(dto))
                .ThrowsAsync(new Core.Application.Exceptions.ValidationException(
                    new[] { new ValidationFailure("Name", "Name is required") }));

            // Act
            Func<Task> act = async () => await service.AddAsync(dto);

            // Assert
            await act.Should().ThrowAsync<Core.Application.Exceptions.ValidationException>();
        }


        [Fact]
        public async Task EditAsync_Should_Update_Category_And_Return_Dto_When_Exists()
        {
            // Arrange
            var service = CreateService();
            var context = new LibraryMSContext(_dbContextOptions);

            var category = new Category { Name = "Old Name" };
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var dto = new EditCategoryDto
            {
                Name = "Updated Name"
            };

            // Act
            var result = await service.EditAsync(category.CategoryId, dto);

            // Assert
            result.Should().NotBeNull();
            result!.CategoryId.Should().Be(category.CategoryId);
            result.Name.Should().Be(dto.Name);
        }

        [Fact]
        public async Task EditAsync_Should_Return_Null_When_Category_Does_Not_Exist()
        {
            // Arrange
            var service = CreateService();
            var dto = new EditCategoryDto { Name = "Non Existing" };

            // Act
            var result = await service.EditAsync(999, dto);

            // Assert
            result.Should().BeNull();
        }


        [Fact]
        public async Task DeleteAsync_Should_Delete_Category_And_Return_True_When_Exists()
        {
            // Arrange
            var service = CreateService();
            var context = new LibraryMSContext(_dbContextOptions);

            var category = new Category { Name = "To Delete" };
            await context.Categories.AddAsync(category);
            await context.SaveChangesAsync();

            // Act
            var result = await service.DeleteAsync(category.CategoryId);

            // Assert
            result.Should().BeTrue();

            var verifyContext = new LibraryMSContext(_dbContextOptions);
            var deleted = await verifyContext.Categories.FindAsync(category.CategoryId);

            deleted.Should().BeNull();
        }


        [Fact]
        public async Task DeleteAsync_Should_Throw_When_Category_Does_Not_Exist()
        {
            // Arrange
            var service = CreateService();

            // Act
            Func<Task> act = async () => await service.DeleteAsync(999);

            // Assert
            await act.Should().ThrowAsync<Core.Application.Exceptions.ApiException>();
        }

    }
}
