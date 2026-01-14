namespace LibraryMS_API.Core.Application.Dtos.Dashboard
{
    public class DashboardStatsDto
    {
        public int TotalBooks { get; set; }
        public int TotalBorrowedRecords { get; set; }
        public int TotalUsers { get; set; }
        public int TotalOverdueBooks { get; set; }
    }
}
