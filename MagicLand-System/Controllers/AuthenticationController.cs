using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MagicLand_System.Controllers
{
    [ApiController]
    public class AuthenticationController : BaseController<AuthenticationController>
    {
        private readonly IUserService _userService;

        public AuthenticationController(ILogger<AuthenticationController> logger, IUserService userService) : base(logger)
        {
            _userService = userService;
        }
        [HttpPost(ApiEndpointConstant.Authentication.AuthenticationEndpoint)]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(UnauthorizedObjectResult))]
        public async Task<IActionResult> Authentication([FromBody] LoginRequest loginRequest)
        {
            var loginResponse = await _userService.Authentication(loginRequest);
            if (loginResponse == null)
            {
                return Unauthorized(new ErrorResponse()
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Error = "Invalid Phone",
                    TimeStamp = DateTime.Now
                });
            }
            return Ok(loginResponse);
        }
        [HttpPost(ApiEndpointConstant.Authentication.AuthenticationEndpointRefreshToken)]
        [ProducesResponseType(typeof(NewTokenResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(UnauthorizedObjectResult))]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest refreshTokenRequest)
        {
            var token = await _userService.RefreshToken(refreshTokenRequest);
            if (token == null)
            {
                return Unauthorized(new ErrorResponse()
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Error = "Authentication/Token is invalid",
                    TimeStamp = DateTime.Now
                });
            }
            return Ok(token);
        }
    }

}

