using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Request.Attendance;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.Services.Interfaces;
using MagicLand_System.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MagicLand_System.Controllers
{
    [ApiController]
    public class AttandanceController : BaseController<AttandanceController>
    {
        private readonly IAttandanceService _attandanceService;
        public AttandanceController(ILogger<AttandanceController> logger,IAttandanceService attandanceService) : base(logger)
        {
            _attandanceService = attandanceService;
        }
        [HttpGet(ApiEndpointConstant.AttandanceEndpoint.LoadAttandance)]
        [CustomAuthorize(Enums.RoleEnum.STAFF)]
        public async Task<IActionResult> LoadAttandance(string scheduleId)
        {
            var result = await _attandanceService.LoadAttandance(scheduleId);
            return Ok(result);
        }
        [HttpPost(ApiEndpointConstant.AttandanceEndpoint.TakeAttandance)]
        [CustomAuthorize(Enums.RoleEnum.STAFF)]
        public async Task<IActionResult> TakeAttandace([FromBody] List<StaffClassAttandanceRequest> requests)
        {
            var isSuccess = await _attandanceService.TakeAttandance(requests);
            if(!isSuccess)
            {
                return BadRequest(new ErrorResponse { Error = "không thể lưu điểm danh ", StatusCode = StatusCodes.Status400BadRequest, TimeStamp = DateTime.Now });
            }
            return Ok("successfully");
        }
    }
}
