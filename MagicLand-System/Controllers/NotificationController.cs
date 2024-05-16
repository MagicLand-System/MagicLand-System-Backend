using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Notifications;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MagicLand_System.Controllers
{
    public class NotificationController : BaseController<NotificationController>
    {
        private readonly INotificationService _notificationService;
        public NotificationController(ILogger<NotificationController> logger, INotificationService notificationService) : base(logger)
        {
            _notificationService = notificationService;
        }

        #region document API Get Notification For User
        /// <summary>
        ///  Truy Suất Toàn Bộ Thông Báo Của Người Dùng Hiện Tại
        /// </summary>
        /// <response code="200">Trả Về Danh Sách Thông Báo</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.NotificationEndpoint.GetNotifications)]
        [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(Exception))]
        [Authorize]
        public async Task<IActionResult> GetNotifications()
        {
            var responses = await _notificationService.GetCurrentUserNotificationsAsync();
            return Ok(responses);
        }

        #region document API Get Notification For Staff
        /// <summary>
        ///  Truy Suất Toàn Bộ Thông Báo Của Staff
        /// </summary>
        /// <response code="200">Trả Về Danh Sách Thông Báo</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.NotificationEndpoint.GetStaffNotifications)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequestObjectResult))]
        [Authorize(Roles = "STAFF")]
        public async Task<IActionResult> GetStaffNotifications()
        {
            var responses = await _notificationService.GetStaffNotificationsAsync();
            return Ok(responses);
        }

        #region document API Update Notification
        /// <summary>
        ///  Cập Nhập Trạng Thái Của Thông Báo Sang Đã Dọc
        /// </summary>
        /// <response code="200">Cập Nhập Thành Công</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpPut(ApiEndpointConstant.NotificationEndpoint.UpdateNotification)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(Exception))]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateNotification([FromQuery] List<Guid> ids)
        {
            if (ids == null || !ids.Any())
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Yêu Cầu Không Hợp Lệ",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }

            var response = await _notificationService.UpdateNotificationAsync(ids);
            return Ok(response);
        }

        #region document API Delete Notification
        /// <summary>
        ///  Xóa Thông Báo
        /// </summary>
        /// <response code="200">Xóa Thành Công</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpDelete(ApiEndpointConstant.NotificationEndpoint.DeleteNotification)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(Exception))]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteNotification([FromQuery] List<Guid> ids)
        {
            if (ids == null || !ids.Any())
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Yêu Cầu Không Hợp Lệ",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }
            var response = await _notificationService.DeleteNotificationAsync(ids);
            return Ok(response);
        }
    }
}
