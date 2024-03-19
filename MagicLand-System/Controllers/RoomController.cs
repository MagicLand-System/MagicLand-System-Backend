using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Request;
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
        [HttpPost(ApiEndpointConstant.RoomEnpoint.GetAll)]
        public async Task<IActionResult> GetAll(FilterRoomRequest? filterRoomRequest)
        {
            var result = await _roomService.GetRoomList(filterRoomRequest);
            return Ok(result);
        }
        [HttpGet(ApiEndpointConstant.RoomEnpoint.RoomByAdmin)]
        public async Task<IActionResult> GetByAdmin(DateTime? startDate , DateTime? endDate, string? searchString,string? slotId)
        {
            var result = await _roomService.GetAdminRoom(startDate,endDate,searchString,slotId);
            return Ok(result);
        }
        [HttpGet(ApiEndpointConstant.RoomEnpoint.RoomByAdminV2)]
        public async Task<IActionResult> GetByAdminV2(DateTime date)
        {
            var result = await _roomService.GetAdminRoomV2(date);
            return Ok(result);
        }
    }
}
