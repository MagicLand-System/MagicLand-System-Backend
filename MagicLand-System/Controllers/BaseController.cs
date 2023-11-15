using MagicLand_System.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MagicLand_System.Controllers
{
    [Route(ApiEndpointConstant.ApiEndpoint)]
    [ApiController]
    public class BaseController<T> : ControllerBase where T : BaseController<T>
    {
        protected ILogger<T> _logger;

        public BaseController(ILogger<T> logger)
        {
            _logger = logger;
        }
    }
}
