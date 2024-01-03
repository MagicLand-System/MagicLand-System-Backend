using Azure.Core;
using MagicLand_System.Constants;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Request.Cart;
using MagicLand_System.PayLoad.Request.Checkout;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Cart;
using MagicLand_System.PayLoad.Response.Class;
using MagicLand_System.PayLoad.Response.Student;
using MagicLand_System.Services.Implements;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using static MagicLand_System.Constants.ApiEndpointConstant;

namespace MagicLand_System.Controllers
{
    public class CheckOutController : BaseController<CheckOutController>
    {

        private readonly IUserService _userService;
        private readonly ICartService _cartService;
        private readonly IClassService _classService;
        private readonly IStudentService _studentService;
        public CheckOutController(ILogger<CheckOutController> logger, IUserService userService, ICartService cartService, IClassService classService, IStudentService studentService) : base(logger)
        {
            _userService = userService;
            _cartService = cartService;
            _classService = classService;
            _studentService = studentService;
        }

        #region document API check-out cart
        /// <summary>
        ///  Check-out now current class selected
        /// </summary>
        /// <param name="request">Store id of current register class and list id of student registered</param>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///        "classId": "c6d70a5f-56ae-4de0-b441-c080da024524"
        ///        "StudentsIdList": {"172c40fe-32e4-43fd-b982-c87afe8b54fa", "f9113f7e-ae51-4f65-a7b4-2348f666787d"}
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Return a bill after progress success</response>
        /// <response code="400">Invalid some value request</response>
        /// <response code="403">Invalid role</response>
        /// <response code="500">Unhandel database error</response>
        #endregion
        [HttpPost(ApiEndpointConstant.User.UserEndPointCheckoutNow)]
        [ProducesResponseType(typeof(BillResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequestObjectResult))]
        [Authorize(Roles = "PARENT")]
        public async Task<IActionResult> CheckoutNow(CheckoutRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            await ValidRequest(request.ClassId, request.StudentsIdList);

            var allStudentSchedules = new List<StudentScheduleResponse>();
            foreach (var task in request.StudentsIdList.Select(async stu => await _studentService
            .GetScheduleOfStudent(stu.ToString())))
            {
                var schedules = await task;
                allStudentSchedules.AddRange(schedules);
       
            }

            if (!await _userService.ValidRegisterAsync(allStudentSchedules, request.ClassId, request.StudentsIdList))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Request meet invalid class standard",
                    StatusCode = StatusCodes.Status500InternalServerError,
                    TimeStamp = DateTime.Now,
                });
            }
            var response = await _userService.CheckoutNowAsync(request);
            return Ok(response);
        }

        #region document API check-out cart
        /// <summary>
        ///  Check-out selected item in cart
        /// </summary>
        /// <param name="cartItemIds">Id of all item in cart want to purchase</param>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///       "cartItemIds" : {"d3407e14-c7fc-49ff-ade3-438bedf415a8", "g3d07e14-ccrc-49ff-ade3-438bedolpkms"}
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Return a bill after progess</response>
        /// <response code="404">Invalid request</response>
        /// <response code="403">Invalid role</response>
        /// <response code="500">Unhandel database error</response>
        #endregion
        [HttpPost(ApiEndpointConstant.CartEnpoint.CheckOutCart)]
        [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
        [Authorize(Roles = "PARENT")]
        public async Task<IActionResult> CheckOutCart([FromBody] List<Guid> cartItemIds)
        {

            var cart = await _cartService.GetDetailCurrentParrentCart();

            var invalidItem = cartItemIds.Except(cart.CartItems.Select(s => s.Id)).ToList();
            if (invalidItem.Any())
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Items not esxit in cart: " + invalidItem.Select(x => x.ToString(" And ")),
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }

            var items = cartItemIds.Select(ci => cart.CartItems.SingleOrDefault(c => c.Id == ci)).ToList();

            foreach (var item in items)
            {
                var allStudentSchedules = new List<StudentScheduleResponse>();
                foreach (var task in item!.Students.Select(async stu => await _studentService
                .GetScheduleOfStudent(stu.Id.ToString())))
                {
                    var schedules = await task;
                    allStudentSchedules.AddRange(schedules);

                }

                if (!await _userService.ValidRegisterAsync(allStudentSchedules, item.Class.Id, item.Students.Select(stu => stu.Id).ToList()))
                {
                    return BadRequest(new ErrorResponse
                    {
                        Error = "Request meet invalid class standard",
                        StatusCode = StatusCodes.Status500InternalServerError,
                        TimeStamp = DateTime.Now,
                    });
                }
            }

            var response = await _cartService.CheckOutCartAsync(items!);
            return Ok(response);
        }

        private async Task<IActionResult> ValidRequest(Guid classId, List<Guid> studentIds)
        {
            if (await _classService.GetClassByIdAsync(classId) == null)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Class Id: {classId} Not Esxit",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }

            var students = await _studentService.GetStudentsOfCurrentParent();
            var invalidStudentIds = studentIds.Except(students.Select(s => s.Id)).ToList();

            if (invalidStudentIds.Any())
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Invalid student Id or you are trying to sign someone else's student: " + invalidStudentIds.Select(x => x.ToString(" And ")),
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }

            return Ok();
        }
    }
}
