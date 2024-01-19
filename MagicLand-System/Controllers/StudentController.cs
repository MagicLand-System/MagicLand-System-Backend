using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Request.Checkout;
using MagicLand_System.PayLoad.Request.Student;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Carts;
using MagicLand_System.PayLoad.Response.Students;
using MagicLand_System.Services.Implements;
using MagicLand_System.Services.Interfaces;
using MagicLand_System.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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
                return NotFound(new ErrorResponse
                {
                    Error = "Not found any class",
                    StatusCode = StatusCodes.Status404NotFound,
                    TimeStamp = DateTime.Now,
                });
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



        #region document API update student
        /// <summary>
        ///  Cho Phép Phụ Huynh Cập Nhập Học Sinh
        /// </summary>
        /// <param name="request">Chứa Thông Tin Của Học Sinh</param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///    "StudentId":"3c1849af-400c-43ca-979e-58c71ce9301d",
        ///    "FullName":"Nguyen Van A",
        ///    "DateOfBirth":"2024/1/19",
        ///    "Gender":"Name",
        ///    "AvatarImage":"url",
        ///    "Email":"avannguyen@gmail.com"
        ///    
        ///}
        /// </remarks>
        /// <response code="200">Trả Về Thông Tin Học Sinh Sau Khi Cập Nhập</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpPut(ApiEndpointConstant.StudentEndpoint.UpdateStudent)]
        [ProducesResponseType(typeof(StudentResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequestObjectResult))]
        [Authorize(Roles = "PARENT")]
        public async Task<IActionResult> UpdateStudent([FromBody] UpdateStudentRequest request)
        {
            var student = (await _studentService.GetStudentsOfCurrentParent()).FirstOrDefault(stu => stu.Id == request.StudentId);
           
            if (student == null)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Id [{request.StudentId}] Của Học Sinh Không Tồn Tại Hoặc Bạn Đang Sử Dụng Id Của Học Sinh Khác Không Phải Con Bạn.",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }

            if (!student.IsActive!.Value)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Id [{request.StudentId}] Của Học Sinh Đã Ngưng Hoạt Động.",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }
            var response = await _studentService.UpdateStudentAsync(request, student);

            return Ok(response);
        }


        #region document API delete student
        /// <summary>
        ///  Chó Phép Phụ Huynh Xóa Thông Tin Học Sinh Khỏi Hệ Thống
        /// </summary>
        /// <param name="id">Id Của Học Sinh </param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///    "Id":"3c1849af-400c-43ca-979e-58c71ce9301d",
        ///    
        ///}
        /// </remarks>
        /// <response code="200">Trả Về Thông Báo Sau Khi Xóa</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpDelete(ApiEndpointConstant.StudentEndpoint.DeleteStudent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequestObjectResult))]
        [Authorize(Roles = "PARENT")]
        public async Task<IActionResult> DeleteStudent(Guid id)
        {
            var student = (await _studentService.GetStudentsOfCurrentParent()).FirstOrDefault(stu => stu.Id == id);

            if (student == null)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Id [{id}] Của Học Sinh Không Tồn Tại Hoặc Bạn Đang Sử Dụng Id Của Học Sinh Khác Không Phải Con Bạn.",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }

            if (!student.IsActive!.Value)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Id [{id}] Của Học Sinh Đã Ngưng Hoạt Động.",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }
            var response = await _studentService.DeleteStudentAsync(student);

            return Ok(response);
        }
    }
}
