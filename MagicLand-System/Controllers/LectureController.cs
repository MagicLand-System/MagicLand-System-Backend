using MagicLand_System.Constants;
using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Request.Attendance;
using MagicLand_System.PayLoad.Request.Evaluates;
using MagicLand_System.PayLoad.Response.Attendances;
using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System.PayLoad.Response.Classes.ForLecturer;
using MagicLand_System.PayLoad.Response.Evaluates;
using MagicLand_System.PayLoad.Response.Lectures;
using MagicLand_System.PayLoad.Response.Quizzes.Result.Student;
using MagicLand_System.PayLoad.Response.Schedules.ForLecturer;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

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
        /// <param name="request">Chứa Id Của Lớp Học, Id Của Học Sinh Cần Điểm Danh Và Thông Tin Liên Quan</param>
        /// <param name="slot">Slot Điểm Danh</param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///    "ClassId":"3c1849af-400c-43ca-979e-58c71ce9301d"
        ///    "SlotTime": "Slot1" ( Slot1: "7:00", Slot2: "9:15", Slot3: "12:00", Slot4: "14:15", Slot5: "16:30", Slot6: "19:00")
        ///    [
        ///       {
        ///          "StudentId":"3c1849af-400c-43ca-979e-58c71ce9301d",
        ///          "IsAttendance": "true",
        ///          "Note": "Học Bù"
        ///        }
        ///    ]
        ///}
        /// </remarks>
        /// <response code="200">Trả Về Thông Báo Sau Khi Điểm Danh</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpPut(ApiEndpointConstant.LectureEndpoint.TakeStudentAttendance)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequestObjectResult))]
        [Authorize(Roles = "LECTURER")]
        public async Task<IActionResult> TakeStudentAttendance([FromBody] AttendanceRequest request, [FromQuery] SlotEnum slot)
        {
            var response = await _studentService.TakeStudentAttendanceAsync(request, slot);

            return Ok(response);
        }

        #region document API Get Student Evaluates
        /// <summary>
        ///  Truy Suất Danh Sách Đánh Giá Của Các Học Sinh Trong Một Lớp 
        /// </summary>
        /// <param name="classId">Id Của Lớp Học</param>
        /// <param name="noSession">Thứ Tự Hiện Của Buổi Học Cần Truy Suất Đánh Giá (Option)</param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///    "classId":"3c1849af-400c-43ca-979e-58c71ce9301d"
        ///    "noSession": 6,
        ///}
        /// </remarks>
        /// <response code="200">Trả Về Danh Sách Đánh Giá Của Các Học Sinh</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.LectureEndpoint.GetStudentEvaluates)]
        [ProducesResponseType(typeof(EvaluateResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequestObjectResult))]
        [Authorize(Roles = "LECTURER")]
        public async Task<IActionResult> GetStudentEvaluate([FromQuery] Guid classId, [FromQuery] int? noSession)
        {
            var response = await _classService.GetStudentEvaluatesAsync(classId, noSession);
            return Ok(response);
        }


        #region document API Get Student Test Result
        /// <summary>
        ///  Truy Suất Danh Sách Các Bài Kiểm Tra Đã Làm Của Các Học Sinh
        /// </summary>
        /// <param name="classId">Id Của Lớp Học</param>
        /// <param name="studentIdList">Id Của Các Học Sinh Cần Truy Suất (Option)</param>
        /// <param name="examIdList">Id Của Các Bài Kiểm Tra Cần Truy Suất (Option)</param>
        /// <param name="isLatestAttempt">Truy Suất Theo Lần Làm Mới Nhất, Mặc Định [Không] (Option)</param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///    "classId":"3c1849af-400c-43ca-979e-58c71ce9301d"
        ///    "studentIdList": [{"4729E1A5-79F9-48A5-B8ED-0A53F963Cc00"}],
        ///    "examIdList": [{"735616C5-B24A-4C16-A30A-A27A570CD6FE"}],
        ///    "isLatestAttempt": false,
        ///}
        /// </remarks>
        /// <response code="200">Trả Về Danh Sách Điểm Kiểm Tra Của Các Học Sinh</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.LectureEndpoint.GetStudentQuizFullyInfor)]
        [ProducesResponseType(typeof(QuizResultWithStudentWork), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequestObjectResult))]
        [Authorize(Roles = "LECTURER")]
        public async Task<IActionResult> GetStudentQuizFullyInfor([FromQuery] Guid classId, [FromQuery] List<Guid>? studentIdList, [FromQuery] List<Guid>? examIdList, [FromQuery] bool isLatestAttempt = false)
        {
            var responses = await _studentService.GetStudentQuizFullyInforAsync(classId, studentIdList, examIdList, isLatestAttempt);
            return Ok(responses);
        }

        #region document API Evaluate Students
        /// <summary>
        ///  Cho Phép Giảng Viên Đánh Giá Buổi Học Của Các Học Sinh Của Một Lớp, Thông Qua Id Của Lớp Và Id Buổi Học
        /// </summary>
        /// <param name="request">Chứa Id Của Lớp Học, Id Của Học Sinh Cần Đánh Giá, Mức Độ Và Ghi Chú Cho Đánh Giá</param>
        /// <param name="noSession">Thứ Tự Hiện Tại Của Buổi Học Cần Đánh Giá</param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///    "ClassId":"3c1849af-400c-43ca-979e-58c71ce9301d"
        ///    "sessionId": "5229E1A5-79F9-48A5-B8ED-0A53F963Cd31"
        ///    [
        ///       {
        ///          "StudentId":"6ab50a00-08ba-483c-bf5d-0d55b05a2c1a",
        ///          "Level": 1, 
        ///          "Note": "Dự Thính",
        ///        }
        ///    ]
        ///}
        /// </remarks>
        /// <response code="200">Trả Về Thông Báo Sau Khi Đánh Giá</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpPut(ApiEndpointConstant.LectureEndpoint.EvaluateStudent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequestObjectResult))]
        [Authorize(Roles = "LECTURER")]
        public async Task<IActionResult> TakeStudentEvaluate([FromBody] EvaluateRequest request, [FromQuery] int noSession)
        {
            var response = await _studentService.TakeStudentEvaluateAsync(request, noSession);
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
        [HttpGet(ApiEndpointConstant.LectureEndpoint.GetStudentAttendance)]
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
        [HttpGet(ApiEndpointConstant.LectureEndpoint.GetStudentAttendanceOfAllClass)]
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
        ///  Truy Suất Các Lớp Học Có Lịch Dạy
        /// </summary>
        /// <response code="200">Trả Về Lịch Dạy Của Các Lớp Gần Nhất</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.LectureEndpoint.GetCurrentClassesSchedule)]
        [ProducesResponseType(typeof(ClassWithSlotOutSideResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequest))]
        [Authorize(Roles = "LECTURER")]
        public async Task<IActionResult> GetCurrentLetureClassesSchedule()
        {
            var responses = await _classService.GetCurrentLectureClassesScheduleAsync();
            if (!responses.Any())
            {
                return NotFound();
            }

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
        [HttpGet(ApiEndpointConstant.LectureEndpoint.GetClassesAttendanceWithDate)]
        [ProducesResponseType(typeof(ScheduleWithAttendanceResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequest))]
        [Authorize(Roles = "LECTURER")]
        public async Task<IActionResult> GetAttendanceOfClassesInDate([FromQuery] Guid classId, [FromQuery] DateTime date)
        {
            var responses = await _classService.GetAttendanceOfClassesInDateAsync(classId, date);
            return Ok(responses);
        }



        #region document API Get Career Lecturer
        /// <summary>
        ///  Truy Suất Các Môn Dạy Học Của Giáo Viên
        /// </summary>
        /// <response code="200">Trả Về Các Môn Dạy Học</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.LectureEndpoint.GetLecturerCareer)]
        [ProducesResponseType(typeof(LecturerCareerResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequest))]
        [AllowAnonymous]
        public async Task<IActionResult> GetLectureCareer()
        {
            var responses = await _userService.GetLecturerCareerAsync();
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
        [HttpGet(ApiEndpointConstant.LectureEndpoint.GetLectureSchedule)]
        [ProducesResponseType(typeof(LectureScheduleResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequest))]
        [Authorize(Roles = "LECTURER")]
        public async Task<IActionResult> GetLectureSchedule([FromQuery] Guid? classId)
        {
            var responses = await _userService.GetLectureScheduleAsync(classId);
            return Ok(responses);
        }


        #region document API Get All Class 
        /// <summary>
        ///  Truy Suất Tất Cả Các Lớp Của Giáo Viên Hiện Tại
        /// </summary>
        /// <response code="200">Trả Về Các Lớp Học Của Giáo Viên| Trả Rỗng Khi Giáo Viên Không Có Lịch</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.LectureEndpoint.GetLecturerClasses)]
        [ProducesResponseType(typeof(ClassWithSlotShorten), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequest))]
        [Authorize(Roles = "LECTURER")]
        public async Task<IActionResult> GetLectureClasses([FromQuery] ClassStatusEnum? status)
        {
            var responses = await _classService.GetCurrentLecturerAllClassesAsync(status);
            return Ok(responses);
        }
    }
}
