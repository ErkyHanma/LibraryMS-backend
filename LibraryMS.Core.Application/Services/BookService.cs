using AutoMapper;
using LibraryMS.Core.Application.Dtos.Base;
using LibraryMS.Core.Application.Dtos.Book;
using LibraryMS.Core.Application.Dtos.Image;
using LibraryMS.Core.Application.Exceptions;
using LibraryMS.Core.Application.Interfaces;
using LibraryMS.Core.Domain.Entities;
using LibraryMS.Core.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LibraryMS.Core.Application.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IValidationService _validationService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IMapper _mapper;

        public BookService(IBookRepository bookRepository, IValidationService validationService, IMapper mapper, ICloudinaryService cloudinaryService)
        {
            _bookRepository = bookRepository;
            _validationService = validationService;
            _cloudinaryService = cloudinaryService;
            _mapper = mapper;
        }

        // Get all books with optional search, sorting, filtering and pagination
        public async Task<PaginatedResult<BookDto>> GetAllAsync(
            string? search, string? category,
            string? order = "desc", bool? isAvailable = false,
            int page = 1, int limit = 10)
        {
            // Validate parameters
            if (page < 1) page = 1;
            if (limit < 1) limit = 10;
            if (limit > 100) limit = 100;

            var query = _bookRepository.GetAllQueryWithInclude(["BookCategories.Category"]);

            // Filter by category name
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(b => b.BookCategories != null &&
                        b.BookCategories.Any(bc => bc.Category != null &&
                            bc.Category.Name.ToLower().Contains(category.ToLower())));
            }

            // Filter by availability
            if (isAvailable == true)
                query = query.Where(b => b.AvailableCopies > 0);

            // Search by title and author
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b =>
                b.Title.ToLower().Contains(search.ToLower()) ||
                b.Author.ToLower().Contains(search.ToLower()));
            }


            // Get total count for pagination
            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(total / (double)limit);

            // Apply ordering 
            query = order?.ToLower() == "asc"
                 ? query.OrderBy(c => c.CreatedAt)
                 : query.OrderByDescending(c => c.CreatedAt);


            var items = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            var listDtos = _mapper.Map<List<BookDto>>(items);

            return new PaginatedResult<BookDto>
            {
                Data = listDtos,
                Meta = new PageMetadata
                {
                    Page = page,
                    Limit = limit,
                    Total = total,
                    TotalPage = totalPages
                }
            };
        }

        public async Task<BookDto?> GetByIdAsync(int id)
        {
            var entity = await _bookRepository
                .GetAllQueryWithInclude(["BookCategories.Category"])
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (entity == null)
            {
                return null;
            }

            BookDto dto = _mapper.Map<BookDto>(entity);
            return dto;

        }

        public async Task<List<BookDto>> GetAllByCategoryIdAsync(int id)
        {
            var listEntities = await _bookRepository
                .GetAllQueryWithInclude(["BookCategories.Category"])
                .Where(b => b.BookCategories != null && b.BookCategories.Any(bc => bc.CategoryId == id))
                .ToListAsync();

            var listEntityDtos = _mapper.Map<List<BookDto>>(listEntities);

            return listEntityDtos;

        }

        public async Task<BookDto?> AddAsync(AddBookDto dto)
        {

            ImageUploadResultDto? newImage = null;

            try
            {

                await _validationService.ValidateAsync(dto);

                newImage = await _cloudinaryService.UploadImageAsync(dto.CoverFile, "LibraryMS/books/covers");

                if (newImage == null)
                    return null;

                Book entity = _mapper.Map<Book>(dto);
                entity.CoverImageUrl = newImage.FileImageUrl;
                entity.CoverImageKey = newImage.FileImageKey;
                Book? returnEntity = await _bookRepository.AddBookWithCategories(entity, dto.CategoryIds);


                if (returnEntity == null)
                {
                    // Cleanup uploaded image if book creation failed
                    await _cloudinaryService.DeleteImageAsync(newImage.FileImageKey);
                    return null;
                }

                return _mapper.Map<BookDto>(returnEntity);
            }
            catch (Exception)
            {

                // delete image upload if anything fails
                if (newImage != null)
                    await _cloudinaryService.DeleteImageAsync(newImage.FileImageKey);

                throw;
            }


        }

        public async Task<BookDto?> EditAsync(int id, EditBookDto dto)
        {
            var existingBook = await _bookRepository.GetByIdAsync(id);

            if (existingBook == null)
                throw ApiException.NotFound($"Book with ID {id} not found");

            ImageUploadResultDto? newImage = null;
            var oldImageKey = existingBook.CoverImageKey;

            try
            {
                // Validate DTO
                await _validationService.ValidateAsync(dto);

                // Upload new image (if provided)
                if (dto.CoverFile != null)
                {
                    newImage = await _cloudinaryService
                        .UploadImageAsync(dto.CoverFile, "LibraryMS/books/covers");
                }

                var book = _mapper.Map<Book>(dto);
                book.BookId = id;
                book.CoverImageUrl = newImage?.FileImageUrl ?? existingBook.CoverImageUrl;
                book.CoverImageKey = newImage?.FileImageKey ?? existingBook.CoverImageKey;

                var updatedBook = await _bookRepository.EditBookWithCategories(book, id, dto.CategoryIds);

                if (updatedBook == null)
                    throw ApiException.BadRequest("Failed to update book");

                // Delete old image AFTER DB success
                if (newImage != null && !string.IsNullOrEmpty(oldImageKey))
                {
                    await _cloudinaryService.DeleteImageAsync(oldImageKey);
                }

                return _mapper.Map<BookDto>(updatedBook);
            }
            catch
            {
                // delete new upload if anything fails
                if (newImage != null)
                    await _cloudinaryService.DeleteImageAsync(newImage.FileImageKey);

                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await _bookRepository.DeleteAsync(id);
            return true;
        }
    }
}
