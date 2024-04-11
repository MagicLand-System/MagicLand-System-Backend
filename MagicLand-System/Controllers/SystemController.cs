﻿using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MagicLand_System.Controllers
{
    [ApiController]
    public class SystemController : BaseController<SystemController>
    {
        private readonly IDashboardService _dashboardService;
        public SystemController(ILogger<SystemController> logger,IDashboardService dashboardService) : base(logger)
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
        [HttpGet("System/GetRegistered")]
        public async Task<IActionResult> GetRegistered(DateTime? startDate,DateTime? endDate) 
        {
            return Ok(await _dashboardService.GetDashboardRegisterResponses(startDate, endDate));
        }
    }
}
