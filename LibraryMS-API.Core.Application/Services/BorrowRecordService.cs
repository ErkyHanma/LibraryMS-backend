using AutoMapper;
using LibraryMS_API.Core.Application.Dtos.Base;
using LibraryMS_API.Core.Application.Dtos.Book;
using LibraryMS_API.Core.Application.Dtos.BorrowRecord;
using LibraryMS_API.Core.Application.Exceptions;
using LibraryMS_API.Core.Application.Interfaces;
using LibraryMS_API.Core.Domain.Entities;
using LibraryMS_API.Core.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LibraryMS_API.Core.Application.Services
{
    public class BorrowRecordService : IBorrowRecordService
    {
        private readonly IBorrowRecordRepository _borrowRecordRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public BorrowRecordService(
            IBorrowRecordRepository borrowRecordRepository,
            IUserService userService,
            IMapper mapper)
        {
            _borrowRecordRepository = borrowRecordRepository;
            _userService = userService;
            _mapper = mapper;
        }


        public async Task<PaginatedResult<BorrowRecordDto>> GetAllAsync(string? searchTerm, string? status, string? order = "desc", int page = 1, int limit = 10)
        {
            // Validate parameters
            if (page < 1) page = 1;
            if (limit < 1) limit = 10;
            if (limit > 100) limit = 100;

            var query = _borrowRecordRepository.GetAllQueryWithInclude(["Book.BookCategories.Category"]);

            // Filter by status
            if (!string.IsNullOrEmpty(status))
                switch (status.ToLower())
                {
                    case "returned":
                        query = query.Where(br => br.ReturnDate != null);
                        break;
                    case "overdue":
                        query = query.Where(br => br.ReturnDate == null && br.DueDate < DateTime.UtcNow);
                        break;
                    case "borrowed":
                        query = query.Where(br => br.ReturnDate == null && br.DueDate >= DateTime.UtcNow);
                        break;
                    default:
                        break;
                }

            // search by book title or author
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(br =>
                    br.Book != null && (br.Book.Title.ToLower().Contains(searchTerm.ToLower()) ||
                     br.Book.Author.ToLower().Contains(searchTerm.ToLower())));
            }

            // Get total 
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


            var listDtos = new List<BorrowRecordDto>();

            foreach (var borrowRecord in items)
            {
                // get for each borrow record the user information
                var userDto = await _userService.GetById(borrowRecord.UserId);

                var dto = new BorrowRecordDto
                {
                    BorrowRecordId = borrowRecord.BorrowRecordId,
                    BorrowDate = borrowRecord.BorrowDate,
                    DueDate = borrowRecord.DueDate,
                    ReturnDate = borrowRecord.ReturnDate,
                    CreatedAt = borrowRecord.CreatedAt,
                    UpdatedAt = borrowRecord.UpdatedAt,
                    Book = _mapper.Map<BookDto>(borrowRecord.Book),
                    User = userDto
                };

                listDtos.Add(dto);
            }

            return new PaginatedResult<BorrowRecordDto>
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

        public async Task<PaginatedResult<BorrowRecordDto>> GetAllByUserIdAsync(string userId, string? searchTerm, string? status, string? order = "desc", int page = 1, int limit = 10)
        {
            // Validate parameters
            if (page < 1) page = 1;
            if (limit < 1) limit = 10;
            if (limit > 100) limit = 100;

            var query = _borrowRecordRepository
               .GetAllQueryWithInclude(["Book.BookCategories.Category"])
               .Where(br => br.UserId == userId);

            // Filter by status
            if (!string.IsNullOrEmpty(status))
                switch (status.ToLower())
                {
                    case "returned":
                        query = query.Where(br => br.ReturnDate != null);
                        break;
                    case "overdue":
                        query = query.Where(br => br.ReturnDate == null && br.DueDate < DateTime.UtcNow);
                        break;
                    case "borrowed":
                        query = query.Where(br => br.ReturnDate == null && br.DueDate >= DateTime.UtcNow);
                        break;
                    default:
                        break;
                }

            // search by book title or author
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(br =>
                    br.Book != null && (br.Book.Title.ToLower().Contains(searchTerm.ToLower()) ||
                     br.Book.Author.ToLower().Contains(searchTerm.ToLower())));
            }

            // Get total
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


            var listDtos = new List<BorrowRecordDto>();

            foreach (var borrowRecord in items)
            {
                // get for each borrow record the user information
                var userDto = await _userService.GetById(borrowRecord.UserId);

                var dto = new BorrowRecordDto
                {
                    BorrowRecordId = borrowRecord.BorrowRecordId,
                    BorrowDate = borrowRecord.BorrowDate,
                    DueDate = borrowRecord.DueDate,
                    ReturnDate = borrowRecord.ReturnDate,
                    CreatedAt = borrowRecord.CreatedAt,
                    UpdatedAt = borrowRecord.UpdatedAt,
                    Book = _mapper.Map<BookDto>(borrowRecord.Book),
                    User = userDto
                };

                listDtos.Add(dto);
            }

            return new PaginatedResult<BorrowRecordDto>
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

        public async Task<BorrowRecordDto?> GetById(int borrowRecordId)
        {

            var borrowRecord = await _borrowRecordRepository
               .GetAllQueryWithInclude(["Book.BookCategories.Category"])
               .FirstOrDefaultAsync(br => br.BorrowRecordId == borrowRecordId);

            if (borrowRecord == null)
                return null;

            // get the borrow record the user information
            var userDto = await _userService.GetById(borrowRecord.UserId);

            var borrowRecordDto = new BorrowRecordDto
            {
                BorrowRecordId = borrowRecord.BorrowRecordId,
                BorrowDate = borrowRecord.BorrowDate,
                DueDate = borrowRecord.DueDate,
                ReturnDate = borrowRecord.ReturnDate,
                CreatedAt = borrowRecord.CreatedAt,
                UpdatedAt = borrowRecord.UpdatedAt,
                Book = _mapper.Map<BookDto>(borrowRecord.Book),
                User = userDto
            };


            return borrowRecordDto;
        }

        public async Task<BorrowRecordDto?> AddBorrowRecordAsync(AddBorrowRecordDto dto)
        {
            var MAX_USER_BORROW_LIMIT = 5;

            // check if user exists
            var userDto = await _userService.GetById(dto.UserId);
            if (userDto == null)
                throw ApiException.NotFound("User not found");

            // check if the user can borrow another book
            var userBorrowedRecordCount = await _borrowRecordRepository
                .GetAllQuery().Where(br => br.UserId == dto.UserId && br.ReturnDate == null).CountAsync();

            if (userBorrowedRecordCount >= MAX_USER_BORROW_LIMIT)
                throw ApiException.BadRequest("User has reached the maximum borrow limit");

            // check if the user has already borrowed the book and not returned it yet
            var borrowedRecord = await _borrowRecordRepository
                .GetAllQuery().FirstOrDefaultAsync(br => br.BookId == dto.BookId && br.UserId == dto.UserId && br.ReturnDate == null);

            if (borrowedRecord != null)
                throw ApiException.BadRequest("User has already borrow this record");

            BorrowRecord borrowRecord = _mapper.Map<BorrowRecord>(dto);
            BorrowRecord? returnEntity = await _borrowRecordRepository.AddAsync(borrowRecord);

            if (returnEntity == null)
                return null;

            return new BorrowRecordDto
            {
                BorrowRecordId = returnEntity.BorrowRecordId,
                BorrowDate = returnEntity.BorrowDate,
                DueDate = returnEntity.DueDate,
                ReturnDate = returnEntity.ReturnDate,
                CreatedAt = returnEntity.CreatedAt,
                UpdatedAt = returnEntity.UpdatedAt,
                Book = _mapper.Map<BookDto>(returnEntity.Book),
                User = userDto
            };
        }

        public async Task<bool> ReturnBorrowedRecordAsync(int borrowRecordId)
        {
            return await _borrowRecordRepository.ReturnBorrowedRecordAsync(borrowRecordId);
        }


    }
}
