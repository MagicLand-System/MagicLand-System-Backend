using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Request.Student;
using MagicLand_System.PayLoad.Response.Students;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.Services.Implements;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MagicLand_System.PayLoad.Request.Attendance;
using MagicLand_System.PayLoad.Response.Attendances;
using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Response.Classes;
using Microsoft.AspNetCore.Http.HttpResults;
using MagicLand_System.Background.BackgroundServiceInterfaces;
using MagicLand_System.PayLoad.Response.Schedules.ForLecturer;

namespace MagicLand_System.Controllers
{
    [ApiController]
    public class LectureController : BaseController<LectureController>
    {
        private readonly IStudentService _studentService;
        private readonly IAttendanceService _attendanceService;
        private readonly IClassService _classService;
        private readonly IUserService _userService;
        public LectureController(ILogger<LectureController> logger, IStudentService studentService, IAttendanceService attendanceService, IClassService classService, IUserService userService) : base(logger)
        {
            _studentService = studentService;
            _attendanceService = attendanceService;
            _classService = classService;
            _userService = userService;
        }

        #region document API Take Student Attendance
        /// <summary>
        ///  Cho Phép Giảng Viên Điểm Danh Các Học Sinh Của Một Lớp Ở Ngày Hiện Tại Thông Qua Id Của Lớp Và Slot Học
        /// </summary>
        /// <param name="request">Chứa Id Của Lớp Học, Id Của Học Sinh Cần Điểm Danh Và Trạng Thái Điểm Danh</param>
        /// <param name="slot">Slot Điểm Danh</param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///    "ClassId":"3c1849af-400c-43ca-979e-58c71ce9301d"
        ///    "SlotTime": "Slot1" ( Slot1: "7:00", Slot2: "9:15", Slot3: "12:00", Slot4: "14:15", Slot5: "16:30", Slot6: "19:00")
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
        [HttpPut(ApiEndpointConstant.LectureEndPoint.TakeStudentAttendance)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequestObjectResult))]
        [Authorize(Roles = "LECTURER")]
        public async Task<IActionResult> TakeStudentAttendance([FromBody] AttendanceRequest request, [FromQuery] SlotEnum slot)
        {
            var response = await _studentService.TakeStudentAttendanceAsync(request, slot);

            return Ok(response);
        }

        #region document API Get Student Attendance
        /// <summary>
        ///  Truy Suất Danh Sách Điểm Danh Của Học Sinh Của Một Lớp Ở Ngày Hiện Tại
        /// </summary>
        /// <param name="classId">Chứa Id Của Lớp Học Hiện Tại Cần Lấy Danh Sách</param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///   "classId":"3c1849af-400c-43ca-979e-58c71ce9301d"
        ///}
        /// </remarks>
        /// <response code="200">Trả Về Danh Sách Điểm Danh</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.LectureEndPoint.GetStudentAttendance)]
        [ProducesResponseType(typeof(AttendanceResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequestObjectResult))]
        [Authorize(Roles = "LECTURER")]
        public async Task<IActionResult> GetStudentAttendance([FromQuery] Guid classId)
        {
            var response = await _studentService.GetStudentAttendanceFromClassInNow(classId);
            return Ok(response);
        }

        #region document API Get Student Attendance Of All Class
        /// <summary>
        ///  Truy Suất Danh Sách Điểm Danh Các Lớp Của Giáo Viên Hiện Tại
        /// </summary>
        /// <response code="200">Trả Về Danh Sách Điểm Danh</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.LectureEndPoint.GetStudentAttendanceOfAllClass)]
        [ProducesResponseType(typeof(AttendanceWithClassResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequestObjectResult))]
        [Authorize(Roles = "LECTURER")]
        public async Task<IActionResult> GetStudentAttendanceOfAllClass()
        {
            var response = await _attendanceService.GetAttendanceOfClassesOfCurrentUserAsync();
            return Ok(response);
        }

        #region document API Get Current Lecture Classes
        /// <summary>
        ///  Truy Suất Slot Lịch Dạy Các Lớp Của Giáo Viên Hiện Tại Có Trong Hôm Nay
        /// </summary>
        /// <response code="200">Trả Về Lịch Dạy Của Giáo Viên Trong Hôm Nay</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.LectureEndPoint.GetCurrentClassesSchedule)]
        [ProducesResponseType(typeof(ClassResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequest))]
        [Authorize(Roles = "LECTURER")]
        public async Task<IActionResult> GetCurrentLetureClassesSchedule()
        {
            var responses = await _classService.GetCurrentLectureClassesScheduleAsync();    
            return Ok(responses);
        }

        #region document API Get All Class Attendance
        /// <summary>
        ///  Truy Suất Danh Sách Điểm Danh Của Một Lớp Của Giáo Viên Hiện Tại Trong Một Ngày Cụ Thể 
        /// </summary>
        /// <param name="classId">Chứa Id Của Lớp Học Cần Lấy Danh Sách</param>
        /// <param name="date">Ngày Cần Truy Suất</param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///   "classId":"3c1849af-400c-43ca-979e-58c71ce9301d",
        ///   "date":"2024-01-29"
        ///}
        /// </remarks>
        /// <response code="200">Trả Về Danh Sách Điểm Danh Thỏa Mãn | Danh Sách Điểm Danh Rỗng Khi Không Có Lịch Trong Ngày Yêu Cầu Hoặc Lớp Chưa Đủ Chỉ Số</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.LectureEndPoint.GetClassesAttendanceWithDate)]
        [ProducesResponseType(typeof(ScheduleWithAttendanceResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequest))]
        [Authorize(Roles = "LECTURER")]
        public async Task<IActionResult> GetCurrentLetureClasses([FromQuery] Guid classId, [FromQuery] DateTime date)
        {
            var responses = await _classService.GetAttendanceOfClassesInDateAsync(classId, date);
            return Ok(responses);
        }



        #region document API Get All Class Schedules
        /// <summary>
        ///  Truy Suất Lịch Giảng Dạy Của Giáo Viên Hiện Tại
        /// </summary>
        /// <response code="200">Trả Về Lịch Dạy Của Giáo Viên | Trả Rỗng Khi Giáo Viên Không Có Lịch</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.LectureEndPoint.GetLectureSchedule)]
        [ProducesResponseType(typeof(LectureScheduleResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequest))]
        [Authorize(Roles = "LECTURER")]
        public async Task<IActionResult> GetLectureSchedule()
        {
            var responses = await _userService.GetLectureScheduleAsync();
            return Ok(responses);
        }
    }
}
