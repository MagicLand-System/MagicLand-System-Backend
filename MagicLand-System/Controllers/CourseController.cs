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

     
        [HttpGet(ApiEndpointConstant.CourseEnpoint.GetAll)]
        [ProducesResponseType(typeof(List<CourseResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnhandledExceptionEventHandler), StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<IActionResult> GetCourses()
        {
            var courses = await _courseService.GetCoursesAsync();
            return Ok(courses);
        }

     
        [HttpGet(ApiEndpointConstant.CourseEnpoint.SearchCourse)]
        [ProducesResponseType(typeof(CourseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnhandledExceptionEventHandler), StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<IActionResult> SearchCourse([FromQuery] string keyWord)
        {
            var courses = await _courseService.SearchCourseAsync(keyWord);
            return Ok(courses);
        }

       
        [HttpGet(ApiEndpointConstant.CourseEnpoint.CourseById)]
        [ProducesResponseType(typeof(CourseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BadHttpRequestException), StatusCodes.Status400BadRequest)]
        [AllowAnonymous]
        public async Task<IActionResult> GetCoureById(Guid id)
        {
            var courses = await _courseService.GetCourseByIdAsync(id);
            return Ok(courses);
        }

      
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
