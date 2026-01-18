using FluentAssertions;
using LibraryMS.Core.Domain.Entities;
using LibraryMS.Infrastructure.Persistence.Contexts;
using LibraryMS.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LibraryMS.Tests.IntegrationTests.Persistence.Repositories
{
    public class CategoryRepositoryTest
    {
        private readonly DbContextOptions<LibraryMSContext> _dbContextOptions;

        public CategoryRepositoryTest()
        {
            _dbContextOptions = new DbContextOptionsBuilder<LibraryMSContext>()
                  .UseInMemoryDatabase(databaseName: $"LibraryMSDb_{Guid.NewGuid()}")
                .Options;
        }


        [Fact]
        public async Task GetAllAsync_Should_Return_All_Categories()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            context.Categories.AddRange(
                new Category { Name = "Fiction" },
                new Category { Name = "Non-Fiction" }
            );
            await context.SaveChangesAsync();
            var repository = new CategoryRepository(context);

            // Act
            var categories = await repository.GetAllListAsync();

            // Assert
            categories.Should().NotBeNull();
            categories.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_List_When_No_Categories()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var repository = new CategoryRepository(context);

            // Act
            var categories = await repository.GetAllListAsync();

            // Assert
            categories.Should().NotBeNull();
            categories.Should().BeEmpty();
        }

        [Fact]
        public void GetAllQuery_Should_Return_IQueryable()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            context.Categories.Add(new Category { Name = "Drama" });
            context.SaveChanges();

            var repository = new CategoryRepository(context);

            // Act
            var query = repository.GetAllQuery();

            // Assert
            query.Should().NotBeNull();
            query.Count().Should().Be(1);
        }

        [Fact]
        public async Task GetById_Should_Return_Category_By_Id()
        {
            // Arrnge
            using var context = new LibraryMSContext(_dbContextOptions);
            Category category = new() { CategoryId = 1, Name = "Action" };
            await context.Categories.AddAsync(category);
            var repository = new CategoryRepository(context);

            // Act
            var result = await repository.GetByIdAsync(category.CategoryId);


            // Assert
            result.Should().NotBeNull();
            result.CategoryId.Should().Be(category.CategoryId);
            result.Name.Should().Be(category.Name);
        }

        [Fact]
        public async Task GetById_Should_Return_Null_When_Category_Not_Found()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var repository = new CategoryRepository(context);

            // Act
            var result = await repository.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_Should_Add_Category_To_Database()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var repository = new CategoryRepository(context);
            var category = new Category { Name = "Science Fiction" };

            // Act
            var result = await repository.AddAsync(category);

            // Assert
            result.Should().NotBeNull();
            result.CategoryId.Should().BeGreaterThan(0);
            result.Name.Should().Be("Science Fiction");

            var categories = await context.Categories.ToListAsync();
            Assert.Single(categories);
        }

        [Fact]
        public async Task AddAsync_Should_Throw_Exception_When_Null()
        {
            using var context = new LibraryMSContext(_dbContextOptions);
            var repository = new CategoryRepository(context);

            // Act
            Func<Task> act = async () => await repository.AddAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                        .WithMessage("Value cannot be null. (Parameter 'entity')");
        }

        [Fact]
        public async Task AddRangeAsync_Should_Add_Multiple_Categories()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var repository = new CategoryRepository(context);

            var categories = new List<Category>
                {
                    new() { Name = "History" },
                    new() { Name = "Biography" }
                };

            // Act
            var result = await repository.AddRangeAsync(categories);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            var dbCategories = await context.Categories.ToListAsync();
            dbCategories.Should().HaveCount(2);
        }

        [Fact]
        public async Task EditAsync_Should_Update_Category_When_Exists()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var category = new Category { Name = "Old Name" };
            await context.Categories.AddAsync(category);

            var repository = new CategoryRepository(context);

            var updatedCategory = new Category { Name = "New Name" };

            // Act
            var result = await repository.EditAsync(category.CategoryId, updatedCategory);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("New Name");
        }

        [Fact]
        public async Task EditAsync_Should_Return_Null_When_Category_Not_Found()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var repository = new CategoryRepository(context);

            var category = new Category { Name = "Does Not Exist" };

            // Act
            var result = await repository.EditAsync(999, category);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_Category_When_Exists()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var category = new Category { Name = "To Delete" };
            await context.Categories.AddAsync(category);
            await context.SaveChangesAsync();

            var repository = new CategoryRepository(context);

            // Act
            await repository.DeleteAsync(category.CategoryId);

            // Assert
            var result = await context.Categories.FindAsync(category.CategoryId);
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Not_Throw_When_Category_Not_Found()
        {
            // Arrange
            using var context = new LibraryMSContext(_dbContextOptions);
            var repository = new CategoryRepository(context);

            // Act
            Func<Task> act = async () => await repository.DeleteAsync(999);

            // Assert
            await act.Should().NotThrowAsync();
        }


    }
}
