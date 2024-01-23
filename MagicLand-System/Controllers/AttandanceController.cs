using MagicLand_System.Constants;
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
        public async Task<IActionResult> LoadAttandance(string classId, DateTime dateTime)
        {
            var result = await _attandanceService.LoadAttandance(classId, dateTime);
            return Ok(result);
        }
    }
}
