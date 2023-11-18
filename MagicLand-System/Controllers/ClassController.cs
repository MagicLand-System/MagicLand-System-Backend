using MagicLand_System.Constants;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.Services.Implements;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        public async Task<IActionResult> GetCourses()
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
        [AllowAnonymous]
        public async Task<IActionResult> GetClassById(Guid id)
        {
            var courses = await _classService.GetClassById(id);
            if(courses == null)
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
    }
}
