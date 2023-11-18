using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.Services.Implements;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MagicLand_System.Controllers
{
    public class CourseController : BaseController<CourseController>
    {
        private readonly ICourseService _courseService;
        public CourseController(ILogger<CourseController> logger, ICourseService courseService) : base(logger)
        {
            _courseService = courseService;
        }

        [HttpGet(ApiEndpointConstant.Course.CourseEnpoint)]
        public async Task<IActionResult> GetCourses()
        {
            var courses = await _courseService.GetCoursesAsync();
            return Ok(courses);
        }

        [HttpGet(ApiEndpointConstant.Course.SearchCourse)]
        public async Task<IActionResult> SearchCourse([FromBody] string keyWord)
        {
            var courses = await _courseService.SearchCourseAsync(keyWord);
            return Ok(courses);
        }

        [HttpGet(ApiEndpointConstant.Course.FilterCourse)]
        public async Task<IActionResult> FilterCourse([FromBody] FilterCourseRequest request)
        {
            var courses = await _courseService.FilterCourseAsync(request);
            return Ok(courses);
        }
    }
}
