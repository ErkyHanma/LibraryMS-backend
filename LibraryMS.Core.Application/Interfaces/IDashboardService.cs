using LibraryMS.Core.Application.Dtos.Dashboard;

namespace LibraryMS.Core.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync();
    }
}