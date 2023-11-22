using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Class;
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

        #region document API Get Classes
        /// <summary>
        ///  Get All Class Existed
        /// </summary>
        /// <response code="200">Return a list of Course statify request</response>
        /// <response code="500">Unhandel database error</response>
        #endregion
        [HttpGet(ApiEndpointConstant.ClassEnpoint.GetAll)]
        [AllowAnonymous]
        public async Task<IActionResult> GetClasses()
        {
            var courses = await _classService.GetClassesAsync();
            return Ok(courses);
        }

        #region document API Get Class By Course Id
        /// <summary>
        ///  Get All Specific Class Base On Course Id
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///        "id": "fded66d4-c3e7-4721-b509-e71feab6723a"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Return a list of class statify request</response>
        /// <response code="400">Id of course not esxist</response>
        #endregion
        [HttpGet(ApiEndpointConstant.ClassEnpoint.ClassByCourseId)]
        [ProducesResponseType(typeof(List<ClassResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
        [AllowAnonymous]
        public async Task<IActionResult> GetClassByCourseId(Guid id)
        {
            var courses = await _classService.GetClassesByCourseIdAsync(id);
            return Ok(courses);
        }



        #region document API Get Class By Id
        /// <summary>
        ///  Get Detail Specific Class By Id
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///        "id": "c6d70a5f-56ae-4de0-b441-c080da024524"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Return detail of class statify request</response>
        /// <response code="400">Id of class not esxist</response>
        #endregion
        [HttpGet(ApiEndpointConstant.ClassEnpoint.ClassById)]
        [AllowAnonymous]
        public async Task<IActionResult> GetClassById(Guid id)
        {
            var courses = await _classService.GetClassByIdAsync(id);
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
