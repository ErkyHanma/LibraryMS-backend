using LibraryMS_API.Core.Application.Dtos.Dashboard;
using LibraryMS_API.Core.Application.Interfaces;
using LibraryMS_API.Core.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LibraryMS_API.Core.Application.Services
{
    // Service for Dashboard related operations
    public class DashboardService : IDashboardService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IBorrowRecordRepository _borrowRecordRepository;
        private readonly IUserService _userService;

        public DashboardService(IBookRepository bookRepository, IBorrowRecordRepository borrowRecordRepository, IUserService userService)
        {
            _bookRepository = bookRepository;
            _borrowRecordRepository = borrowRecordRepository;
            _userService = userService;
        }

        // Method to get dashboard statistics
        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var totalBorrowedRecordQuery = _borrowRecordRepository.GetAllQuery().Where(br => br.ReturnDate == null);


            var totalBooks = await _bookRepository.GetAllQuery().CountAsync();
            var TotalBorrowedRecords = await totalBorrowedRecordQuery.CountAsync();
            var TotalOverdueBooks = await totalBorrowedRecordQuery.Where(br => br.DueDate < DateTime.UtcNow).CountAsync();
            var totalUsers = await _userService.GetTotalUserCountAsync();


            return new DashboardStatsDto
            {
                TotalBooks = totalBooks,
                TotalBorrowedRecords = TotalBorrowedRecords,
                TotalOverdueBooks = TotalOverdueBooks,
                TotalUsers = totalUsers
            };
        }

    }
}
