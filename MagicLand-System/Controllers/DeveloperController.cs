using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Request.Evaluates;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Custom;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MagicLand_System.Controllers
{
    [ApiController]
    public class DeveloperController : BaseController<DeveloperController>
    {
        private readonly IDeveloperService _developerService;
        private readonly IQuizService _quizService;
        private readonly IClassService _classService;
        private readonly IUserService _userService;
        private readonly IStudentService _studentService;
        public DeveloperController(ILogger<DeveloperController> logger, IDeveloperService developerService, IQuizService quizService, IClassService classService, IUserService userService, IStudentService studentService) : base(logger)
        {
            _developerService = developerService;
            _quizService = quizService;
            _classService = classService;
            _userService = userService;
            _studentService = studentService;
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

        [HttpGet(ApiEndpointConstant.DeveloperEndpoint.GetStudentAuthenAndExam)]
        [ProducesResponseType(typeof(StudentAuthenAndExam), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequestObjectResult))]
        [Authorize(Roles = "DEVELOPER")]
        public async Task<IActionResult> GetStudentAndExamByClassId([FromQuery] Guid classId)
        {
            var studentAuthens = new List<LoginResponse>();
            var students = await _classService.GetAllStudentInClass(classId.ToString());

            var studentAccounts = await _developerService.GetStudentAccount(students.Select(stu => stu.StudentId).ToList());
            foreach (var acc in studentAccounts)
            {
                var studentAuthen = await _userService.Authentication(new LoginRequest { Phone = acc.AccountPhone });
                studentAuthens.Add(studentAuthen);
            }
            var exams = await _quizService.LoadExamOfClassByClassIdAsync(classId, null);

            var response = new StudentAuthenAndExam
            {
                StudentAuthen = studentAuthens,
                Exams = exams,
            };

            return Ok(response);
        }
    }
}
