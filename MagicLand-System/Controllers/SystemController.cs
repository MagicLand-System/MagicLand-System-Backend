using MagicLand_System.Domain.Models;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Quartz;

namespace MagicLand_System.Controllers
{
    [ApiController]
    public class SystemController : BaseController<SystemController>
    {
        private readonly IDashboardService _dashboardService;
        private readonly IConfiguration _configuration;
        private readonly ISchedulerFactory _schedulerFactory;

        public SystemController(ILogger<SystemController> logger, IDashboardService dashboardService, IConfiguration configuration, ISchedulerFactory schedulerFactory) : base(logger)
        {
            _dashboardService = dashboardService;
            _configuration = configuration;
            _schedulerFactory = schedulerFactory;
        }

        [HttpPost("/System/SetTime")]
        public async Task<IActionResult> SetTime([FromBody] DateTime date)
        {
            var today = DateTime.Today;
            var day = (date.Date - today).Days;
            var hours = date.TimeOfDay.Hours;
            var minutes = date.TimeOfDay.Minutes;

            _configuration.GetSection("DateNumber:Days").Value = day.ToString();
            _configuration.GetSection("DateNumber:Hours").Value = hours.ToString();
            _configuration.GetSection("DateNumber:Minutes").Value = minutes.ToString();

            var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            var json = System.IO.File.ReadAllText(appSettingsPath);
            dynamic jsonObj = JsonConvert.DeserializeObject(json)!;

            jsonObj["DateNumber"]["Days"] = day.ToString();
            jsonObj["DateNumber"]["Hours"] = hours.ToString();
            jsonObj["DateNumber"]["Minutes"] = minutes.ToString();

            string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
            System.IO.File.WriteAllText(appSettingsPath, output);

            var scheduler = await _schedulerFactory.GetScheduler();
            var jobKey = new JobKey("DailyUpdateJob");

            if (await scheduler.CheckExists(jobKey))
            {
                await scheduler.TriggerJob(jobKey);
                return Ok("Setting Time And Trigger Success");
            }
            else
            {
                return NotFound("Setting Time And Trigger Failed");
            }
        }

        [HttpPost("/System/ResetTime")]
        public IActionResult ResetTime()
        {
            _configuration.GetSection("DateNumber:Days").Value = "0";
            _configuration.GetSection("DateNumber:Hours").Value = "0";
            _configuration.GetSection("DateNumber:Minutes").Value = "0";

            var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            var json = System.IO.File.ReadAllText(appSettingsPath);
            dynamic jsonObj = JsonConvert.DeserializeObject(json)!;

            jsonObj["DateNumber"]["Days"] = "0";
            jsonObj["DateNumber"]["Hours"] = "0";
            jsonObj["DateNumber"]["Minutes"] = "0";

            string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
            System.IO.File.WriteAllText(appSettingsPath, output);

            return Ok("Reset Time Success");
        }

        [HttpGet("/System/GetTime")]
        public IActionResult GetTime()
        {
            return Ok(_dashboardService.GetTime());
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
