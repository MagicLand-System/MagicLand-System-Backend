using MagicLand_System.Constants;
using MagicLand_System.Services.Implements;
using MagicLand_System.Services.Interfaces;
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
        public async Task<IActionResult> GetAll()
        {
            var result = await _roomService.GetRoomList();
            return Ok(result);
        }
    }
}
