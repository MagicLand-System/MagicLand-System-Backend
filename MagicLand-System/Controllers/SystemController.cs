using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MagicLand_System.Controllers
{
    [ApiController]
    public class SystemController : BaseController<SystemController>
    {
        public SystemController(ILogger<SystemController> logger) : base(logger)
        {
        }
        [HttpGet("/System/GetTime")]
        public async Task<IActionResult> GetTime()
        {
            return Ok(DateTime.Now);
        }
    }
}
