using MagicLand_System.Constants;
using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Request.Attendance;
using MagicLand_System.PayLoad.Request.Evaluates;
using MagicLand_System.PayLoad.Response.Custom;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MagicLand_System.Controllers
{
    [ApiController]
    public class DeveloperController : BaseController<DeveloperController>
    {
        private readonly IDeveloperService _developerService;
        private readonly IQuizService _quizService;
        public DeveloperController(ILogger<DeveloperController> logger, IDeveloperService developerService, IQuizService quizService) : base(logger)
        {
            _developerService = developerService;
            _quizService = quizService;
        }

        [HttpPut(ApiEndpointConstant.DeveloperEndpoint.TakeFullAttendanceAndEvaluate)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequestObjectResult))]
        [Authorize(Roles = "DEVELOPER")]
        public async Task<IActionResult> TakeStudentAttendance([FromQuery] Guid classId, [FromQuery] int percentageAbsent, [FromBody] List<EvaluateDataRequest> evaluateNote)
        {
            var response = await _developerService.TakeFullAttendanceAndEvaluateAsync(classId, percentageAbsent, evaluateNote);

            return Ok(response);
        }

        //[HttpGet(ApiEndpointConstant.DeveloperEndpoint.)]
        [ProducesResponseType(typeof(StudentAuthenAndExam), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequestObjectResult))]
        [Authorize(Roles = "DEVELOPER")]
        public async Task<IActionResult> GetStudentAndExamByClassId([FromQuery] Guid classId)
        {
            //var response = await _quizService.loadE.(classId);

            //return Ok(response);
            return Ok();
        }
    }
}
