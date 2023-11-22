using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Request.Cart;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Cart;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MagicLand_System.Controllers
{
    [ApiController]
    public class CartController : BaseController<CartController>
    {
        private readonly ICartService _cartService;
        private readonly IClassService _classService;
        private readonly IStudentService _studentService;
        public CartController(ILogger<CartController> logger, ICartService cartService, IClassService classService, IStudentService studentService) : base(logger)
        {
            _cartService = cartService;
            _classService = classService;
            _studentService = studentService;
        }

        #region document API modify cart
        /// <summary>
        /// Add a class and students register in class to cart Or, update a student registered in current cart item
        /// </summary>
        /// <param name="cartRequest">Store all student id and class id register</param>
        /// <returns>A cart after modify action</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///        "studentIds": [ "3fa85f64-5717-4562-b3fc-2c963f66afa6" , "f9113f7e-ae51-4f65-a7b4-2348f666787d"],
        ///        "classId": "74b1eb4c-33ab-4882-9b6d-c0c6b4fd1678"
        ///     }
        ///     Or
        ///     {
        ///        "studentIds": [],
        ///        "classId": "74b1eb4c-33ab-4882-9b6d-c0c6b4fd1678"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Return a cart after modify success</response>
        /// <response code="400">Invalid request</response>
        /// <response code="403">Invalid role</response>
        /// <response code="500">Unhandel database commit error</response>
        #endregion
        [HttpPost(ApiEndpointConstant.CartEnpoint.ModifyCart)]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
        [Authorize(Roles = "PARENT")]
        public async Task<IActionResult> ModifyCart([FromBody] CartRequest cartRequest)
        {
            if (await _classService.GetClassByIdAsync(cartRequest.ClassId) == null)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Class Id Not Esxit",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }

            var students = await _studentService.GetStudentsOfCurrentParent();
            var invalidStudentIds = cartRequest.StudentIds.Except(students.Select(s => s.Id)).ToList();

            if (invalidStudentIds.Any())
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Invalid Student Id or you are trying to sign someone else's student: " + invalidStudentIds.Select(x => x.ToString()),
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }
            var result = await _cartService.ModifyCartOffCurrentParentAsync(cartRequest.StudentIds, cartRequest.ClassId);
            return Ok(result);
        }


        [HttpGet(ApiEndpointConstant.CartEnpoint.GetCart)]
        [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
        [Authorize(Roles = "PARENT")]
        public async Task<IActionResult> GetCart()
        {
            var cart = await _cartService.GetCartOfCurrentParentAsync();
            return Ok(cart);
        }

        #region document API delete item in cart
        /// <summary>
        ///   Delete current item in cart
        /// </summary>
        /// <param name="id">Id of current item in cart </param>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///        "itemId": "77982AA8-5DFE-41AE-3776-08DBEB2BCC68"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Delete Success</response>
        /// <response code="403">Invalid role</response>
        /// <response code="500">Unhandel database error</response>
        #endregion
        [HttpDelete(ApiEndpointConstant.CartEnpoint.DeleteCartItem)]
        [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
        [Authorize(Roles = "PARENT")]
        public async Task<IActionResult> DeleteCartItem(Guid id)
        {
            var result = await _cartService.DeleteItemInCartOfCurrentParentAsync(id);
            if (!result)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Cart Item Id Not Esxited",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }
            return Ok("Delete Success");
        }
    }
}
