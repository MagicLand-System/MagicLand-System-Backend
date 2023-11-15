using MagicLand_System.Constants;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MagicLand_System.Controllers
{
    [ApiController]
    public class UserController : BaseController<UserController>
    {
        private readonly IUserService _userService;
        public UserController(ILogger<UserController> logger,IUserService userService) : base(logger)
        {
            _userService = userService;
        }
        [HttpGet(ApiEndpointConstant.User.UsersEndpoint)]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetUsers();
            return Ok(users);
        }
    }
}
