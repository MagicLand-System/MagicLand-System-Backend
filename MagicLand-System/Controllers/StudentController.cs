﻿using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Request.Student;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System.PayLoad.Response.Students;
using MagicLand_System.Services.Interfaces;
using MagicLand_System.Validators;
using Microsoft.AspNetCore.Mvc;

namespace MagicLand_System.Controllers
{
    [ApiController]
    public class StudentController : BaseController<StudentController>
    {
        private readonly IStudentService _studentService;
        public StudentController(ILogger<StudentController> logger, IStudentService studentService) : base(logger)
        {
            _studentService = studentService;
        }
        [HttpPost(ApiEndpointConstant.StudentEndpoint.StudentEnpointCreate)]
        [CustomAuthorize(Enums.RoleEnum.PARENT)]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequestObjectResult))]
        public async Task<IActionResult> AddStudent(CreateStudentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var isSuccess = await _studentService.AddStudent(request);
            if (!isSuccess)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Insert to db failed",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }
            return Ok(new { Message = "Create Successfully" });
        }
        [HttpGet(ApiEndpointConstant.StudentEndpoint.StudentEndpointGetClass)]
        [ProducesResponseType(typeof(StudentClassResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(NotFoundResult))]
        [CustomAuthorize(Enums.RoleEnum.PARENT)]
        public async Task<IActionResult> GetClassFromStudent([FromQuery] string studentId, [FromQuery] string status = null)
        {
            var response = await _studentService.GetClassOfStudent(studentId, status);
            if (response == null || response.Count == 0)
            {
               return Ok(new List<ClassResExtraInfor>());
            }
            return Ok(response);
        }
        [HttpGet(ApiEndpointConstant.StudentEndpoint.StudentGetSchedule)]
        [ProducesResponseType(typeof(StudentScheduleResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(NotFoundResult))]
        [CustomAuthorize(Enums.RoleEnum.PARENT)]
        public async Task<IActionResult> GetScheduleFromStudent([FromQuery] string studentId)
        {
            var response = await _studentService.GetScheduleOfStudent(studentId);
            if (response == null || response.Count == 0)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "Not found any schedule",
                    StatusCode = StatusCodes.Status404NotFound,
                    TimeStamp = DateTime.Now,
                });
            }
            return Ok(response);
        }
        [HttpGet(ApiEndpointConstant.StudentEndpoint.StudentGetCurrentChildren)]
        [ProducesResponseType(typeof(StudentScheduleResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(NotFoundResult))]
        [CustomAuthorize(Enums.RoleEnum.PARENT)]
        public async Task<IActionResult> GetStudentFromCurentUser()
        {
            var response = await _studentService.GetStudentsOfCurrentParent();
            if (response == null || response.Count == 0)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "Not found any children",
                    StatusCode = StatusCodes.Status404NotFound,
                    TimeStamp = DateTime.Now,
                });
            }
            return Ok(response);
        }

    }
}
