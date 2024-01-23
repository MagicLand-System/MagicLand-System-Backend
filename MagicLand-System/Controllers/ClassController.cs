    using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Request.Class;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System.Services.Interfaces;
using MagicLand_System.Validators;
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
        ///  Truy Suất Toàn Bộ Lớp Học
        /// </summary>
        /// <response code="200">Trả Về Danh Sách Lớp Học</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.ClassEnpoint.GetAll)]
        [AllowAnonymous]
        public async Task<IActionResult> GetClasses()
        {
            var classes = await _classService.GetClassesAsync();
            return Ok(classes);
        }

        #region document API Get Class By Course Id
        /// <summary>
        ///  Truy Suất Toàn Bộ Lớp Của Một Khóa
        /// </summary>
        /// <param name="id">Id Của Khóa Học</param>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///        "id": "fded66d4-c3e7-4721-b509-e71feab6723a"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Trả Về Danh Sách Thỏa Mãn</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.ClassEnpoint.ClassByCourseId)]
        [ProducesResponseType(typeof(ClassResExtraInfor), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
        [AllowAnonymous]
        public async Task<IActionResult> GetClassByCourseId(Guid id)
        {
            var courses = await _classService.GetClassesByCourseIdAsync(id);
            return Ok(courses);
        }



        #region document API Get Class By Id
        /// <summary>
        ///  Truy Suất Lớp Thông Qua Id Của Lớp
        /// </summary>
        /// <remarks>
        /// <param name="id">Id Của Lớp Học</param>
        /// Sample request:
        ///
        ///     {
        ///        "id": "c6d70a5f-56ae-4de0-b441-c080da024524"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Trả Về Danh Sách Thỏa Mãn</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
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
                    Error = $"Id [{id}] Của Lớp Không Tồn Tại",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }
            return Ok(courses);
        }

        #region document API Filter Class
        /// <summary>
        ///  Tìm Kiếm Hoặc Lọc Lớp Theo Tiêu Chí
        /// </summary>
        /// <param name="keyWords">Cho Các Lớp Thỏa Mãn Các Từ Khóa</param>
        /// <param name="leastNumberStudent">Cho Lớp Có Giới Hạn Tối Thiểu Nhiều Hơn Hoặc Bằng Gía Trị Này</param>
        /// <param name="limitStudent">Cho Lớp Có Giới Hạn Tối Đa Thấp Hơn Hoặc Bằng Gía Trị Này</param>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///        "keyWords": "Basic", "online", "11/25/2023", "ho chi minh", "prn231"
        ///        "leastNumberStudent": 10
        ///        "limitStudent": 30
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Trả Về Danh Sách Thỏa Mãn</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.ClassEnpoint.FilterClass)]
        [AllowAnonymous]
        public async Task<IActionResult> FilterClass(
            [FromQuery] List<string>? keyWords,
            [FromQuery] int? leastNumberStudent,
            [FromQuery] int? limitStudent)
        {
            var classes = await _classService.FilterClassAsync(keyWords, leastNumberStudent, limitStudent);
            return Ok(classes);
        }
        [HttpPost(ApiEndpointConstant.ClassEnpoint.AddClass)]
        [CustomAuthorize(Enums.RoleEnum.STAFF)]
        public async Task<IActionResult> AddClass([FromBody] CreateClassRequest request)
        {
            var isSuccess = await _classService.CreateNewClass(request);
            if (!isSuccess) 
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Add Class is wrong",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }
            return Ok(new { Message = "Create Successfully" });
        }
        [HttpGet(ApiEndpointConstant.ClassEnpoint.GetAllV2)]
        [AllowAnonymous]
        public async Task<IActionResult> GetStaffClass([FromQuery] string? searchString , [FromQuery] string? status)
        {
            var courses = await _classService.GetAllClass(searchString,status);
            return Ok(courses);
        }
        [HttpGet(ApiEndpointConstant.ClassEnpoint.ClassByIdV2)]
        [AllowAnonymous]
        public async Task<IActionResult> GetDetailByStaff(string id)
        {
            var matchclass = await _classService.GetClassDetail(id);
            return Ok(matchclass);
        }
        [HttpGet(ApiEndpointConstant.ClassEnpoint.StudentInClass)]
        [AllowAnonymous]
        public async Task<IActionResult> GetStudentInClass(string id)
        {
            var matchClass = await _classService.GetAllStudentInClass(id);
            if (matchClass == null)
            {
                return NotFound(new ErrorResponse
                {
                    TimeStamp = DateTime.Now,
                    Error = "Class not has any student",
                    StatusCode = StatusCodes.Status404NotFound,
                });
            }
            return Ok(matchClass);  
        }
        [HttpGet(ApiEndpointConstant.ClassEnpoint.AutoCreateClassEndPoint)]
        [AllowAnonymous]
        public async Task<IActionResult> AutoCreate(string courseId)
        {
            var result = await _classService.AutoCreateClassCode(courseId);
            return Ok(new {ClassCode = result});
        }
        [HttpPut(ApiEndpointConstant.ClassEnpoint.UpdateClass)]
        [CustomAuthorize(Enums.RoleEnum.STAFF)]
        public async Task<IActionResult> UpdateStudent([FromRoute] string id,[FromBody] UpdateClassRequest request)
        {
            var isSuccess = await _classService.UpdateClass(id, request);   
            if (!isSuccess)
            {
                return BadRequest(new ErrorResponse
                {
                    TimeStamp = DateTime.Now,
                    Error = "không thể insert",
                    StatusCode = StatusCodes.Status400BadRequest,
                });
            }
            return Ok("success");
        }
        [HttpGet(ApiEndpointConstant.ClassEnpoint.SessionLoad)]
        public async Task<IActionResult> LoadSession(string classId)
        {
            var result = await _classService.GetClassProgressResponsesAsync(classId);
            return Ok(result);
        }
    }
}   
