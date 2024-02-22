using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Request.Course;
using MagicLand_System.PayLoad.Response.Notifications;
using MagicLand_System.PayLoad.Response.Syllabuses;
using MagicLand_System.Services.Implements;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
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
     
        [HttpPost(ApiEndpointConstant.Syllabus.AddSyllabus)]
        public async Task<IActionResult> InsertCourse([FromBody] OverallSyllabusRequest request)
        {
            var isSuccess = await _syllabusService.AddSyllabus(request);
            return Ok(isSuccess);
        }

        #region document API Get Syllabus By Course Id
        /// <summary>
        ///  Truy Suất Giáo Trình Thông Qua Id Của Khóa Học
        /// </summary>
        /// <param name="id">Id Của Khóa Học</param>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///        "Id": "c6d70a5f-56ae-4de0-b441-c080da024524"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Trả Về Giáo Trình</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.Syllabus.LoadByCourse)]
        [ProducesResponseType(typeof(SyllabusResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(Exception))]
        public async Task<IActionResult> LoadSyllabusByCourseId([FromQuery] Guid id)
        {
            var course = await _syllabusService.LoadSyllabusByCourseIdAsync(id);
            return Ok(course);
        }

        #region document API Get Syllabus By Id
        /// <summary>
        ///  Truy Suất Giáo Trình Thông Qua Id
        /// </summary>
        /// <param name="id">Id Của Giáo Trình Cần Truy Suất</param>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///        "Id": "c6d70a5f-56ae-4de0-b441-c080da024524"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Trả Về Giáo Trình</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.Syllabus.LoadSyllabus)]
        [ProducesResponseType(typeof(SyllabusResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(Exception))]
        [AllowAnonymous]
        public async Task<IActionResult> LoadSyllabusById([FromQuery] Guid id)
        {
            var courses = await _syllabusService.LoadSyllabusByIdAsync(id);
            return Ok(courses);
        }
    }
}
