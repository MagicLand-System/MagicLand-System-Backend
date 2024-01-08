using MagicLand_System.Constants;
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
        [AllowAnonymous]
        public async Task<IActionResult> GetCourses()
        {
            var courses = await _courseService.GetCoursesAsync();
            return Ok(courses);
        }

        #region document API Search Courses
        /// <summary>
        ///  Get all courses contain Name key word
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
        [AllowAnonymous]
        public async Task<IActionResult> SearchCourse([FromQuery] string keyWord)
        {
            var courses = await _courseService.SearchCourseByNameAsync(keyWord);
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
        /// <param name="minYearsOld">for course age of student must bigger than this age</param>
        /// <param name="maxYearsOld">for course age of student must lower than this age</param>
        /// <param name="minNumberSession">for course must have a sessions lower to this number</param>
        /// <param name="maxNumberSession">for course must have a sessions greater to this number, if leave null then the value will be max int</param>
        /// <param name="minPrice">for course have price higher than this price</param>
        /// <param name="maxPrice">for course have price lower than this price,  if leave null then the value will be max double</param>
        /// <param name="subject">for all course belong subject</param>
        /// <param name="rate">for all course have euqal or higher than this rate</param>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///        "subject": "Math",
        ///        "minYearsOld": 3,
        ///        "maxYearsOld": 7,
        ///        "minNumberSession": 10,
        ///        "maxNumberSession": 20,
        ///        "minPrice": 2000000,
        ///        "maxPrice": 8000000,
        ///        "rate": 5
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Return a detail of course statify request</response>
        /// <response code="400">Some Field Is Not Valid</response>
        /// <response code="500">Unhandel database error</response>
        #endregion
        [HttpGet(ApiEndpointConstant.CourseEnpoint.FilterCourse)]
        [AllowAnonymous]
        public async Task<IActionResult> FilterCourse(
            [FromQuery] string subject,
            [FromQuery] int minYearsOld = 0,
            [FromQuery] int maxYearsOld = 120,
            [FromQuery] int? minNumberSession = 0,
            [FromQuery] int? maxNumberSession = null,
            [FromQuery] double minPrice = 0,
            [FromQuery] double? maxPrice = null,
            [FromQuery] int? rate = null)
        {
            var courses = await _courseService.FilterCourseAsync(minYearsOld, maxYearsOld, minNumberSession, maxNumberSession, minPrice, maxPrice, subject, rate);
            return Ok(courses);
        }
        [HttpGet(ApiEndpointConstant.CourseEnpoint.GetCourseCategory)]
        public async Task<IActionResult> GetCourseCategory()
        {
            var categories = await _courseService.GetCourseCategories();
            return Ok(categories);
        }
    }
}
