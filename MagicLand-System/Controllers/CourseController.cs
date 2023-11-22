using MagicLand_System.Constants;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Response.Course;
using MagicLand_System.Services.Implements;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MagicLand_System.Controllers
{
    [ApiController]
    public class CourseController : BaseController<CourseController>
    {
        private readonly ICourseService _courseService;
        public CourseController(ILogger<CourseController> logger, ICourseService courseService) : base(logger)
        {
            _courseService = courseService;
        }

        #region document API Get Courses
        /// <summary>
        ///  Get All Courses Existed
        /// </summary>
        /// <response code="200">Return a list of Course statify request</response>
        /// <response code="500">Unhandel database error</response>
        #endregion
        [HttpGet(ApiEndpointConstant.CourseEnpoint.GetAll)]
        [ProducesResponseType(typeof(List<CourseResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnhandledExceptionEventHandler), StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<IActionResult> GetCourses()
        {
            var courses = await _courseService.GetCoursesAsync();
            return Ok(courses);
        }

        #region document API Search Courses
        /// <summary>
        ///  Get All Courses By Name
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///        "keyWord": "basic math"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Return a list of class statific request</response>
        /// <response code="500">Unhandel database error</response>
        #endregion
        [HttpGet(ApiEndpointConstant.CourseEnpoint.SearchCourse)]
        [ProducesResponseType(typeof(CourseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnhandledExceptionEventHandler), StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<IActionResult> SearchCourse([FromQuery] string keyWord)
        {
            var courses = await _courseService.SearchCourseAsync(keyWord);
            return Ok(courses);
        }

        #region document API Get Course By Id
        /// <summary>
        ///  Get Specific Detail Course By Id
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///        "id": "fded66d4-c3e7-4721-b509-e71feab6723a"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Return a detail of course</response>
        /// <response code="400">Id of course not esxist</response>
        /// <response code="500">Unhandel database error</response>
        #endregion
        [HttpGet(ApiEndpointConstant.CourseEnpoint.CourseById)]
        [ProducesResponseType(typeof(CourseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BadHttpRequestException), StatusCodes.Status400BadRequest)]
        [AllowAnonymous]
        public async Task<IActionResult> GetCoureById(Guid id)
        {
            var courses = await _courseService.GetCourseByIdAsync(id);
            return Ok(courses);
        }

        #region document API Filter Courses
        /// <summary>
        ///  Get Specific List Of Course By KeyWord And Filter Options
        /// </summary>
        /// <param name="keyword">for course must contains the key word </param>
        /// <param name="minYearsOld">for course age of student must bigger than this</param>
        /// <param name="maxYearsOld">for course age of student must lower than this</param>
        /// <param name="numberOfSession">for course must have a sessions equal to this</param>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///        "keyword": "basic math"
        ///        "minYearsOld": 3
        ///        "maxYearsOld": 7
        ///        "numberOfSession": 6
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Return a detail of course statify request</response>
        /// <response code="400">Some Field Is Not Valid</response>
        /// <response code="500">Unhandel database error</response>
        #endregion
        [HttpGet(ApiEndpointConstant.CourseEnpoint.FilterCourse)]
        [ProducesResponseType(typeof(CourseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BadHttpRequestException), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(UnhandledExceptionEventHandler), StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<IActionResult> FilterCourse([FromQuery] string? keyword = null, [FromQuery] int? minYearsOld = null, [FromQuery] int? maxYearsOld = null , [FromQuery] int? numberOfSession = null)
        {
            var courses = await _courseService.FilterCourseAsync(keyword, minYearsOld, maxYearsOld,numberOfSession);
            return Ok(courses);
        }
    }
}
