using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MagicLand_System.Controllers
{
    [ApiController]
    public class SlotController : BaseController<SlotController>
    {
        private readonly ISlotService _slotService;
        public SlotController(ILogger<SlotController> logger, ISlotService slotService) : base(logger)
        {
            _slotService = slotService;
        }
        [HttpGet(ApiEndpointConstant.SlotEndpoint.GetAll)]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        public async Task<IActionResult> GetSlots()
        {
            var slots = await _slotService.GetSlots();
            if (slots == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "not found any slot",
                    StatusCode = StatusCodes.Status404NotFound,
                    TimeStamp = DateTime.Now,
                });
            }
            return Ok(slots);
        }
    }
}
