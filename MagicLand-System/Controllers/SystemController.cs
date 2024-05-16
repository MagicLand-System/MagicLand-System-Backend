using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MagicLand_System.Controllers
{
    [ApiController]
    public class SystemController : BaseController<SystemController>
    {
        private readonly IDashboardService _dashboardService;
        public SystemController(ILogger<SystemController> logger, IDashboardService dashboardService) : base(logger)
        {
            _dashboardService = dashboardService;
        }
        [HttpGet("/System/GetTime")]
        public async Task<IActionResult> GetTime()
        {
            return Ok(DateTime.Now);
        }
        [HttpGet("System/GetNumberOfUser")]
        public async Task<IActionResult> GetNumber()
        {
            return Ok(await _dashboardService.GetOfMemberResponse());
        }
        [HttpGet("System/GetRevenue")]
        public async Task<IActionResult> GetRevenue(DateTime? startDate, DateTime? endDate)
        {
            return Ok(await _dashboardService.GetRevenueDashBoardResponse(startDate, endDate));
        }
        [HttpGet("System/GetRegistered")]
        public async Task<IActionResult> GetRegistered(string quarter, string? courseId)
        {
            return Ok(await _dashboardService.GetDashboardRegisterResponses(quarter, courseId));
        }
        [HttpGet("System/GetFavoriteCourse")]
        public async Task<IActionResult> GetFavoriteCourse(DateTime? startDate, DateTime? endDate)
        {
            return Ok(await _dashboardService.GetFavoriteCourseResponse(startDate, endDate));
        }
    }
}
