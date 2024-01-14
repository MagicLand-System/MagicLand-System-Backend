using Azure.Core;
using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Request.Checkout;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Carts;
using MagicLand_System.PayLoad.Response.Students;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

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

        #region document API check-out now
        /// <summary>
        ///  Check-out Single or Multiple register Students into current Classes selected
        /// </summary>
        /// <param name="requests">Store list of current register Classes and list Id of Student in class</param>
        /// <remarks>
        /// Sample request:
        ///[
        ///     {
        ///        "ClassId": "c6d70a5f-56ae-4de0-b441-c080da024524"
        ///        "StudentIdList": {"3fa85f64-5717-4562-b3fc-2c963f66afa6"}
        ///     },
        ///     {
        ///        "ClassId": "1c2ag2g5-kgae-ud3p-bf4a-aaaw1a023gaa"
        ///        "StudentIdList": {"172c40fe-32e4-43fd-b982-c87afe8b54fa", "f9113f7e-ae51-4f65-a7b4-2348f666787d"}
        ///     }
        ///]
        /// </remarks>
        /// <response code="200">Return a bill after progress Success</response>
        /// <response code="400">Invalid some value request</response>
        /// <response code="403">Invalid role</response>
        /// <response code="500">Unhandel database error</response>
        #endregion
        [HttpPost(ApiEndpointConstant.User.UserEndPointCheckout)]
        [ProducesResponseType(typeof(BillResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequestObjectResult))]
        [Authorize(Roles = "PARENT")]
        public async Task<IActionResult> CheckoutV1(List<CheckoutRequest> requests)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            foreach(var request in requests)
            {
                var result = await ValidRequest(request.ClassId, request.StudentIdList);

                if (result is not OkResult)
                {
                    return result;
                }

                var allStudentSchedules = new List<StudentScheduleResponse>();
                foreach (var task in request.StudentIdList.Select(async stu => await _studentService
                .GetScheduleOfStudent(stu.ToString())))
                {
                    var schedules = await task;
                    allStudentSchedules.AddRange(schedules);

                }

                if (!await _userService.ValidRegisterAsync(allStudentSchedules, request.ClassId, request.StudentIdList))
                {
                    return BadRequest(new ErrorResponse
                    {
                        Error = "Request meet invalid class Standard",
                        StatusCode = StatusCodes.Status500InternalServerError,
                        TimeStamp = DateTime.Now,
                    });
                }
            }
            var response = await _userService.CheckoutAsync(requests);

            return Ok(response);
        }

        #region document API check-out cart
        /// <summary>
        ///  Check-out all selected item request from Cart
        /// </summary>
        /// <param name="cartItemIdList">Id of all item in Cart want to check-out</param>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///       "CartItemIdList" : {"d3407e14-c7fc-49ff-ade3-438bedf415a8", "g3d07e14-ccrc-49ff-ade3-438bedolpkms"}
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Return a bill after progess Success</response>
        /// <response code="404">Invalid request</response>
        /// <response code="403">Invalid role</response>
        /// <response code="500">Unhandel database error</response>
        #endregion
        [HttpPost(ApiEndpointConstant.CartEnpoint.CheckOutCart)]
        [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
        [Authorize(Roles = "PARENT")]
        public async Task<IActionResult> CheckoutV2([FromBody] List<Guid> cartItemIdList)
        {
            var result = await ValidCartItem(cartItemIdList);
            var items = result as OkObjectResult;

            if (items == null)
            {
                return result;
            }

            var requests = new List<CheckoutRequest>();

            foreach (var item in (List<CartItemResponse>)items.Value!)
            {
                var allStudentSchedules = new List<StudentScheduleResponse>();

                foreach (var task in item!.Students.Select(async stu => await _studentService
                .GetScheduleOfStudent(stu.StudentId.ToString())))
                {
                    var schedules = await task;
                    allStudentSchedules.AddRange(schedules);

                }

                if (!await _userService.ValidRegisterAsync(allStudentSchedules, item.Class.ClassId, item.Students.Select(stu => stu.StudentId).ToList()))
                {
                    return BadRequest(new ErrorResponse
                    {
                        Error = "Request meet invalid Class standard",
                        StatusCode = StatusCodes.Status500InternalServerError,
                        TimeStamp = DateTime.Now,
                    });
                }

                var request = new CheckoutRequest
                {
                    StudentIdList = item.Students.Select(s => s.StudentId).ToList(),
                    ClassId = item.Class.ClassId,
                };

                requests.Add(request);

            }
           
            var response = await _userService.CheckoutAsync(requests);
            await _cartService.DeleteItemInCartOfCurrentParentAsync(cartItemIdList);

            return Ok(response);
        }


        private async Task<IActionResult> ValidCartItem(List<Guid> cartItemIds)
        {
            var cart = await _cartService.GetDetailCurrentParrentCart();

            var invalidItem = cartItemIds.Except(cart.CartItems.Select(s => s.ItemId)).ToList();
            if (invalidItem.Any())
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Cart items Id not esxit in Cart: [" + string.Join(" And ", invalidItem.ToArray()) + "]",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }

            var items = cartItemIds.Select(ci => cart.CartItems.Single(c => c.ItemId == ci)).ToList();

            var emptyStudentItem = items.Where(x => x.Students.Count() == 0 || x.Class == null).ToList();
            if (emptyStudentItem.Count() > 0)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "There are one or more Cart item Ids without Class and Student registration: [" +
                    string.Join(" And ", emptyStudentItem.Select(x => x.ItemId).ToArray()) + "]",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }

            return Ok(items);
        }
        private async Task<IActionResult> ValidRequest(Guid classId, List<Guid> studentIds)
        {

            if (await _classService.GetClassByIdAsync(classId) == null)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Class Id: [{classId}] Not Esxit",
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
                    Error = "Invalid Student Id or you are trying to sign someone else's Student: [" + invalidStudentIds.Select(x => x.ToString(" And ")) + "]",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }

            Guid duplicateStudentId = studentIds.GroupBy(x => x)
               .Where(list => list.Count() > 1)
               .Select(list => list.First())
               .SingleOrDefault();


            if (duplicateStudentId != default)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Your are request assign Student [{students.Where(x => x.Id == duplicateStudentId).Single().FullName}] " +
                    "more than twice into Class",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }

            return Ok();
        }
    }
}
