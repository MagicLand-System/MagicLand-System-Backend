using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Request.Attendance;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Attendances;
using MagicLand_System.Services.Implements;
using MagicLand_System.Services.Interfaces;
using MagicLand_System.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MagicLand_System.Controllers
{
    [ApiController]
    public class AttendanceController : BaseController<AttendanceController>
    {
        private readonly IAttendanceService _attendanceService;
        public AttendanceController(ILogger<AttendanceController> logger, IAttendanceService attendanceService) : base(logger)
        {
            _attendanceService = attendanceService;
        }
        [HttpGet(ApiEndpointConstant.AttendanceEnpoint.LoadAttandance)]
        [CustomAuthorize(Enums.RoleEnum.STAFF)]
        public async Task<IActionResult> LoadAttandance(string scheduleId, string? searchString)
        {
            var result = await _attendanceService.LoadAttandance(scheduleId, searchString);
            return Ok(result);
        }
        [HttpPost(ApiEndpointConstant.AttendanceEnpoint.TakeAttandance)]
        [CustomAuthorize(Enums.RoleEnum.STAFF)]
        public async Task<IActionResult> TakeAttandace([FromBody] List<StaffClassAttandanceRequest> requests)
        {
            var isSuccess = await _attendanceService.TakeAttandance(requests);
            if (!isSuccess)
            {
                return BadRequest(new ErrorResponse { Error = "không thể lưu điểm danh ", StatusCode = StatusCodes.Status400BadRequest, TimeStamp = DateTime.Now });
            }
            return Ok("successfully");
        }

        #region document API Get Attendance Of Class By Class Id
        /// <summary>
        ///  Truy Suất Danh Sách Điểm Danh Của Một Lớp Thông Qua Id Của Lớp
        /// </summary>
        /// <param name="id">Chứa Id Của Lớp Học Cần Truy Suất</param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///    "id":"3c1849af-400c-43ca-979e-58c71ce9301d",
        ///}
        /// </remarks>
        /// <response code="200">Trả Danh Sách Điểm Danh Của Một Lớp</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.AttendanceEnpoint.GetAttendanceOfClass)]
        [ProducesResponseType(typeof(AttendanceWithClassResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequestObjectResult))]
        [AllowAnonymous]
        public async Task<IActionResult> GetAttendanceOfClass([FromRoute] Guid id)
        {
            var response = await _attendanceService.GetAttendanceOfClassAsync(id);

            return Ok(response);
        }

        #region document API Get Attendance Of Classes
        /// <summary>
        ///  Truy Suất Danh Sách Điểm Danh Của Toàn Bộ Lớp Học
        /// </summary>
        /// <response code="200">Trả Danh Sách Điểm Danh Của Các Lớp</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.AttendanceEnpoint.GetAttendanceOfClasses)]
        [ProducesResponseType(typeof(AttendanceWithClassResponse), StatusCodes.Status200OK)]
        [AllowAnonymous]
        public async Task<IActionResult> GetAttendanceOfClasses()
        {
            var response = await _attendanceService.GetAttendanceOfClassesAsync();

            return Ok(response);
        }

        #region document API Get Attendance Of Student
        /// <summary>
        ///  Truy Suất Danh Sách Điểm Danh Của Các Lớp Của Một Học Sinh Thông Qua Id Của Học Sinh
        /// </summary>
        /// <param name="id">Chứa Id Của Học Sinh Cần Truy Suất</param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///    "id":"3c1849af-400c-43ca-979e-58c71ce9301d",
        ///}
        /// </remarks>
        /// <response code="200">Trả Danh Sách Điểm Danh Của Các Lớp Thỏa Mãn</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.AttendanceEnpoint.GetAttendanceOfStudent)]
        [ProducesResponseType(typeof(AttendanceWithClassResponse), StatusCodes.Status200OK)]
        [AllowAnonymous]
        public async Task<IActionResult> GetAttendanceOfStudent([FromRoute] Guid id)
        {
            var response = await _attendanceService.GetAttendanceOfClassStudent(id);

            return Ok(response);
        }
    }
}
