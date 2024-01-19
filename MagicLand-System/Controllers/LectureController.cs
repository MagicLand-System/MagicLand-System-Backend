﻿using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Request.Student;
using MagicLand_System.PayLoad.Response.Students;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.Services.Implements;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MagicLand_System.PayLoad.Request.Attendance;

namespace MagicLand_System.Controllers
{
    [ApiController]
    public class LectureController : BaseController<LectureController>
    {
        private readonly IUserService _userService;
        private readonly IStudentService _studentService;
        public LectureController(ILogger<LectureController> logger, IUserService userService, IStudentService studentService) : base(logger)
        {
            _userService = userService;
            _studentService = studentService;
        }

        #region document API Take Student Attendance
        /// <summary>
        ///  Cho Phép Giảng Viên Điểm Danh Học Sinh
        /// </summary>
        /// <param name="request">Chứa Id Của Lớp Học Hiện Tại, Id Của Học Sinh Cần Điểm Danh Và Trạng Thái Điểm Danh</param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///    "ClassId":"3c1849af-400c-43ca-979e-58c71ce9301d"
        ///    [
        ///       {
        ///          "StudentId":"3c1849af-400c-43ca-979e-58c71ce9301d",
        ///          "IsAttendance": "true"
        ///        }
        ///    ]
        ///}
        /// </remarks>
        /// <response code="200">Trả Về Thông Báo Sau Khi Điểm Danh</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpPut(ApiEndpointConstant.StudentEndpoint.UpdateStudent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequestObjectResult))]
        [Authorize(Roles = "LECTURER")]
        public async Task<IActionResult> TakeStudentAttendance([FromBody] AttendanceRequest request)
        {
            var response = await _studentService.TakeStudentAttendanceAsync(request);

            return Ok(response);
        }

        #region document API Get Student Attendance
        /// <summary>
        ///  Truy Suất Danh Sách Điểm Danh Của Học Sinh Của Một Lớp Ở Ngày Hiện Tại
        /// </summary>
        /// <param name="request">Chứa Id Của Lớp Học Hiện Tại, Id Của Học Sinh Cần Điểm Danh Và Trạng Thái Điểm Danh</param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///    "ClassId":"3c1849af-400c-43ca-979e-58c71ce9301d"
        ///    [
        ///       {
        ///          "StudentId":"3c1849af-400c-43ca-979e-58c71ce9301d",
        ///          "IsAttendance": "true"
        ///        }
        ///    ]
        ///}
        /// </remarks>
        /// <response code="200">Trả Về Thông Báo Sau Khi Điểm Danh</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpPut(ApiEndpointConstant.StudentEndpoint.UpdateStudent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequestObjectResult))]
        [Authorize(Roles = "LECTURER")]
        public async Task<IActionResult> GetStudentAttendance([FromBody] AttendanceRequest request)
        {
            var response = await _studentService.TakeStudentAttendanceAsync(request);

            return Ok(response);
        }
    }
}
