using MagicLand_System.Constants;
using MagicLand_System.Domain.Models;
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
        [HttpGet(ApiEndpointConstant.User.UsersEndpoint)]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetUsers();
            return Ok(users);
        }
        [HttpGet(ApiEndpointConstant.User.UserEndPointExist)]
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
            return Ok(new { Message = $"Tồn tại user có số điện thoại {phone}" , Role = isExist.Role});
        }
        [HttpGet(ApiEndpointConstant.User.UserEndPointGetCurrentUser)]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(UnauthorizedObjectResult))]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await _userService.GetCurrentUser();
            if (user == (null, null))
            {
                return Unauthorized(new ErrorResponse
                {
                    Error = "Authentication/Accesstoken is invalid",
                    StatusCode = StatusCodes.Status401Unauthorized,
                    TimeStamp = DateTime.Now,
                });
            }
            if(user.Item1 != null)
            {
                return Ok(user.Item1);
            }
            return Ok(user.Item2);
        }
        [HttpPost(ApiEndpointConstant.User.UserEndPointRegister)]
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
        [HttpPost(ApiEndpointConstant.User.UserEndPointGetLecturer)]
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
        [HttpPut(ApiEndpointConstant.User.UpdateUser)]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequestObjectResult))]
        [Authorize(Roles = "PARENT")]
        public async Task<IActionResult> UpdateUser([FromBody] UserRequest request)
        {
            var response = await _userService.UpdateUserAsync(request);

            return Ok(response);
        }

    }
}
