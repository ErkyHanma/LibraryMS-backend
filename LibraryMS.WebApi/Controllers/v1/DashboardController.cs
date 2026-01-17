using Asp.Versioning;
using LibraryMS.Core.Application.Dtos.Dashboard;
using LibraryMS.Core.Application.Interfaces;
using LibraryMS.Core.Domain.Common.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryMS.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize(Roles = $"{nameof(Roles.Admin)}")]
    public class DashboardController : BaseController
    {

        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DashboardStatsDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDashboardStatsCard()
        {
            var dashboardStats = await _dashboardService.GetDashboardStatsAsync();
            return Ok(dashboardStats);
        }
    }
}
