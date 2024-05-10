using MagicLand_System.Constants;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Request.User;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Users;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MagicLand_System.Controllers
{
    [ApiController]
    public class UserController : BaseController<UserController>
    {
        private readonly IUserService _userService;
        public UserController(ILogger<UserController> logger, IUserService userService) : base(logger)
        {
            _userService = userService;
        }

        #region document API Add New User System
        /// <summary>
        ///  Cho Phép Thêm Mới Người Dùng Hệ Thống
        /// </summary>
        /// <param name="request">Chứa Thông Tin Người Dùng</param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///    "UserName":"Nguyen Van A",
        ///    "UserPhone":"+84971822092",
        ///    "Role":"STAFF",
        ///    "LecturerCareerId":"2F1AB569-D516-4A46-9A55-61DBCD6B3692",
        ///}
        /// </remarks>
        /// <response code="200">Trả Về Thông Báo</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpPost(ApiEndpointConstant.UserEndpoint.AddUser)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequestObjectResult))]
        //[Authorize(Roles = "STAFF")]
        public async Task<IActionResult> AddUser([FromBody] UserAccountRequest request)
        {
            var response = await _userService.AddUserAsync(request);
            if (response)
            {
                return Ok("Tạo Thành Công");
            }
            return BadRequest();
        }

        [HttpGet(ApiEndpointConstant.UserEndpoint.RootEndpoint)]
        public async Task<IActionResult> GetUsers([FromQuery] string? keyWord, [FromQuery] RoleEnum? role)
        {
            if(role == RoleEnum.DEVELOPER)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Quyền Này Không Được Cấp Phép Tạo",
                    StatusCode = StatusCodes.Status404NotFound,
                    TimeStamp = DateTime.Now,
                });
            }
            var users = await _userService.GetUsers(keyWord, role);
            return Ok(users);
        }
        [HttpGet(ApiEndpointConstant.UserEndpoint.CheckExist)]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        public async Task<IActionResult> CheckUserExist([FromQuery] string phone)
        {
            var isExist = await _userService.CheckUserExistByPhone(phone);
            if (!isExist.IsExist)
            {
                return NotFound(new ErrorResponse
                {
                    Error = $"Không tìm thấy user có số điện thoại {phone}",
                    StatusCode = StatusCodes.Status404NotFound,
                    TimeStamp = DateTime.Now,
                });
            }
            return Ok(new { Message = $"Tồn tại user có số điện thoại {phone}", Role = isExist.Role });
        }
        [HttpGet(ApiEndpointConstant.UserEndpoint.GetCurrentUser)]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(UnauthorizedObjectResult))]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await _userService.GetCurrentUser();
            if (user == null)
            {
                return Unauthorized(new ErrorResponse
                {
                    Error = "Authentication/Accesstoken is invalid",
                    StatusCode = StatusCodes.Status401Unauthorized,
                    TimeStamp = DateTime.Now,
                });
            }

            return Ok(user);
        }
        [HttpPost(ApiEndpointConstant.UserEndpoint.Register)]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequestObjectResult))]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var isSuccess = await _userService.RegisterNewUser(request);
            if (!isSuccess)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Insert processing was wrong at somewhere",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }
            return Ok(new { Message = "Created Successfully" });
        }
        [HttpPost(ApiEndpointConstant.UserEndpoint.GetLecturer)]
        [ProducesResponseType(typeof(LecturerResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(NotFoundObjectResult))]
        public async Task<IActionResult> GetLecturers(FilterLecturerRequest? request)
        {
            var users = await _userService.GetLecturers(request);
            if (users == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "not found any lecturers",
                    StatusCode = StatusCodes.Status401Unauthorized,
                    TimeStamp = DateTime.Now,
                });
            }
            return Ok(users);
        }

        #region document API update User
        /// <summary>
        ///  Cho Phép Phụ Huynh Cập Nhập Thông Tin Của Mình
        /// </summary>
        /// <param name="request">Chứa Thông Tin Cần Cập Nhập</param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///    "FullName":"Nguyen Van A",
        ///    "DateOfBirth":"2024/1/19",
        ///    "Gender":"Name",
        ///    "AvatarImage":"url",
        ///    "Email":"avannguyen@gmail.com",
        ///    "City":"Ho Chi Minh",
        ///    "District":"9",
        ///    "Street":"D7"  
        ///}
        /// </remarks>
        /// <response code="200">Trả Về Thông Tin Người Dùng Sau Khi Cập Nhập</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpPut(ApiEndpointConstant.UserEndpoint.UpdateUser)]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequestObjectResult))]
        [Authorize(Roles = "PARENT")]
        public async Task<IActionResult> UpdateUser([FromBody] UserRequest request)
        {
            var response = await _userService.UpdateUserAsync(request);

            return Ok(response);
        }
        [HttpGet(ApiEndpointConstant.UserEndpoint.GetByAdmin)]
        public async Task<IActionResult> GetByAdmin(DateTime? startDate , DateTime? endDate , string? searchString, string? slotId)
        {
            var result = await _userService.GetAdminLecturerResponses(startDate, endDate, searchString,slotId);
            return Ok(result);
        }
        [HttpGet(ApiEndpointConstant.UserEndpoint.GetByPhone)]
        public async Task<IActionResult> GetUserFromPhone(string phone)
        {
            var users = await _userService.GetUserFromPhone(phone);
            return Ok(users);
        }
        [HttpGet(ApiEndpointConstant.UserEndpoint.GetStudent)]
        public async Task<IActionResult> GetStudents(string classId,string phone)
        {
            var users = await _userService.GetStudents(classId,phone);
            return Ok(users);
        }
        [HttpGet(ApiEndpointConstant.UserEndpoint.GetFromName)]
        public async Task<IActionResult> GetUserFromName(string name)
        {
            var users = await _userService.GetUserFromName(name);   
            return Ok(users);
        }
        [HttpGet(ApiEndpointConstant.UserEndpoint.GetStudentInfor)]
        public async Task<IActionResult> GetStudentInfor(string? name, DateTime? birthdate, string? id)
        {
            var users = await _userService.GetFromNameAndBirthDate(name,birthdate,id);
            return Ok(users);
        }
        [HttpGet(ApiEndpointConstant.UserEndpoint.GetClassOfStudent)]
        public async Task<IActionResult> GetClassOfStudent(string studentId, string? status, string? searchString,DateTime? dateTime)
        {
            var users = await _userService.GetClassOfStudent(studentId,status,searchString,dateTime);
            return Ok(users);
        }
        [HttpGet(ApiEndpointConstant.UserEndpoint.GetScheduleOfStudent)]
        public async Task<IActionResult> GetScheduleOfStudent(string studentId,DateTime date)
        {
            var users = await _userService.GetScheduleOfStudentInDate(studentId,date);
            return Ok(users);
        }
        [HttpGet(ApiEndpointConstant.UserEndpoint.GetSessionOfStudent)]
        public async Task<IActionResult> GetContentsOfSession(string sessionId)
        {
            var users = await _userService.GetStudentSession(sessionId);
            return Ok(users);
        }
        [HttpGet(ApiEndpointConstant.UserEndpoint.GetListSessionOfStudent)]
        public async Task<IActionResult> GetListContentsOfSession(string classId, string studentId, DateTime? date)
        {
            var users = await _userService.GetStudentSessionAsync(classId,studentId,date);
            return Ok(users);
        }
    }
}
