using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MagicLand_System.Controllers
{
    [ApiController]
    public class ClassController : BaseController<ClassController>
    {
        private readonly IClassService _classService;
        public ClassController(ILogger<ClassController> logger, IClassService classService) : base(logger)
        {
            _classService = classService;
        }

        [HttpGet(ApiEndpointConstant.ClassEnpoint.GetAll)]
        [AllowAnonymous]
        public async Task<IActionResult> GetClasses()
        {
            var courses = await _classService.GetClassesAsync();
            return Ok(courses);
        }

        [HttpGet(ApiEndpointConstant.ClassEnpoint.ClassByCourseId)]
        [ProducesResponseType(typeof(ClassResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequestObjectResult))]
        [AllowAnonymous]
        public async Task<IActionResult> GetClassByCourseId(Guid id)
        {
            var courses = await _classService.GetClassesByCourseIdAsync(id);
            return Ok(courses);
        }

        [HttpGet(ApiEndpointConstant.ClassEnpoint.ClassById)]
        [ProducesResponseType(typeof(ClassResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequestObjectResult))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        public async Task<IActionResult> GetClassById(Guid id)
        {
            var courses = await _classService.GetClassById(id);
            if (courses == null)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Id Of Class Not Existed",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }
            return Ok(courses);
        }
        #region document API Filter Class
        /// <summary>
        ///  Search or Filter class by specific key word and option filter
        /// </summary>
        /// <param name="keyWords">for class it must be contains all key word</param>
        /// <param name="minPrice">for class price must bigger than this price</param>
        /// <param name="maxPrice">for class price must lower than this price</param>
        /// <param name="limitStudent">for class have the limit student equal to this</param>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///        "keyWords": "Basic", "online", "11/25/2023", "ho chi minh"
        ///        "minPrice": 80000
        ///        "maxPrice": 120000
        ///        "limitStudent": 30
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Return a list of class statific request</response>
        /// <response code="400">Some field request not valid</response>
        #endregion
        [HttpGet(ApiEndpointConstant.ClassEnpoint.FilterClass)]
        [ProducesResponseType(typeof(ClassResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
        [AllowAnonymous]
        public async Task<IActionResult> FilterClass(
            [FromQuery] List<string>? keyWords,
            [FromQuery] double? minPrice,
            [FromQuery] double? maxPrice,
            [FromQuery] int? limitStudent)
        {
            var classes = await _classService.FilterClassAsync(keyWords, minPrice, maxPrice, limitStudent);
            return Ok(classes);
        }
    }
}
