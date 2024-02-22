using MagicLand_System.Constants;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MagicLand_System.Controllers
{

    [ApiController]
    public class SyllabusController : BaseController<SyllabusController>
    {
        private readonly ISyllabusService _syllabusService;
        public SyllabusController(ILogger<SyllabusController> logger,ISyllabusService syllabusService) : base(logger)
        {
            _logger = logger;
            _syllabusService = syllabusService;
        }
        [HttpGet(ApiEndpointConstant.Syllabus.GetByCourse)]
        public async Task<IActionResult> Get(string CouseId)
        {
            var course = await _syllabusService.GetSyllasbusResponse(CouseId);
            return Ok(course);
        }
        [HttpGet(ApiEndpointConstant.Syllabus.GetAll)]
        public async Task<IActionResult> GetAll(string? keyword)
        {
            var course = await _syllabusService.GetAllSyllabus(keyword);
            return Ok(course);
        }
    }
}
