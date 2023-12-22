using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.Services.Implements;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MagicLand_System.Controllers
{
    [ApiController]
    public class RoomController : BaseController<RoomController>
    {
        private readonly IRoomService _roomService;

        public RoomController(ILogger<RoomController> logger,IRoomService roomService) : base(logger)
        {
            _roomService = roomService;
        }
        [HttpGet(ApiEndpointConstant.RoomEnpoint.GetAll)]
        [AllowAnonymous]
        public async Task<IActionResult> GetClasses()
        {
            var rooms = await _roomService.GetRoomList();
            if(rooms == null || rooms.Count == 0)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "Not found any room",
                    StatusCode = StatusCodes.Status404NotFound,
                    TimeStamp = DateTime.Now,
                });
            }
            return Ok(rooms);
        }
    }
}
