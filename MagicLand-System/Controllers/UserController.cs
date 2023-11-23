﻿using MagicLand_System.Constants;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Request.Checkout;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
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
        [HttpGet(ApiEndpointConstant.User.UserEndPointExist)]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        public async Task<IActionResult> CheckUserExist([FromQuery] string phone) 
        {
            var isExist = await _userService.CheckUserExistByPhone(phone);
            if (!isExist)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "User with phone not exist",
                    StatusCode = StatusCodes.Status404NotFound,
                    TimeStamp = DateTime.Now,
                }); 
            }
            return Ok(new {Message =  "Phone has exist"});
        }
        [HttpGet(ApiEndpointConstant.User.UserEndPointGetCurrentUser)]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(UnauthorizedObjectResult))]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await _userService.GetCurrentUser();
            if(user == null)
            {
                return Unauthorized(new ErrorResponse
                {
                    Error = "Authentication/Accesstoken is invalid",
                    StatusCode = StatusCodes.Status401Unauthorized,
                    TimeStamp = DateTime.Now,
                });
            }
            return Ok(user);
        }
        [HttpPost(ApiEndpointConstant.User.UserEndPointRegister)]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequestObjectResult))]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest();
            }
            var isSuccess = await _userService.RegisterNewUser(request);
            if (!isSuccess) 
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Insert processing was wrong at somewhere",
                    StatusCode= StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }
            return Ok(new {Message = "Created Successfully"});
        }
        [HttpPost(ApiEndpointConstant.User.UserEndPointCheckoutNow)]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequestObjectResult))]
        public async Task<IActionResult> CheckoutNow(CheckoutRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var isSuccess = await _userService.CheckoutNow(request);
            if (!isSuccess)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Insert processing was wrong at somewhere",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }
            return Ok(new { Message = "Checkout Successfully" });
        }

    }
}
