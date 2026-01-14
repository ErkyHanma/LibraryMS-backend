using LibraryMS_API.Core.Application.Dtos.Dashboard;

namespace LibraryMS_API.Core.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync();
    }
}