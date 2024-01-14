using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Request.Cart;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Carts;
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
        private readonly ICourseService _courseService;
        public CartController(ILogger<CartController> logger, ICartService cartService, IClassService classService, IStudentService studentService, ICourseService courseService) : base(logger)
        {
            _cartService = cartService;
            _classService = classService;
            _studentService = studentService;
            _courseService = courseService;
        }


        #region document API modify favorite
        /// <summary>
        /// Allows adding Course into Favorite List 
        /// </summary>
        /// <param name="courseId">Store favorite Course Id </param>
        /// <returns>A cart after modify action</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///        "CourseId": "74b1eb4c-33ab-4882-9b6d-c0c6b4fd1678"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Return a cart after Modify Success</response>
        /// <response code="400">Invalid request</response>
        /// <response code="403">Invalid role</response>
        /// <response code="500">Unhandel database commit error</response>
        #endregion
        [HttpPost(ApiEndpointConstant.CartEnpoint.AddCourseFavoriteList)]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
        [Authorize(Roles = "PARENT")]
        public async Task<IActionResult> AddCourseFavoriteList([FromQuery] Guid courseId)
        {
            if (await _courseService.GetCourseByIdAsync(courseId) == null)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Course Id Not Esxit",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }

            var result = await _cartService.AddCourseFavoriteOffCurrentParentAsync(courseId);

            return Ok(result);
        }


        #region document API modify cart
        /// <summary>
        /// Allows adding Class with or without Students into Cart (Favorite Class)
        /// </summary>
        /// <param name="cartRequest">Store Class Id and Students Id</param>
        /// <returns>A cart after modify action</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///        "StudentIdList": [ "3fa85f64-5717-4562-b3fc-2c963f66afa6" , "f9113f7e-ae51-4f65-a7b4-2348f666787d"],
        ///        "ClassId": "74b1eb4c-33ab-4882-9b6d-c0c6b4fd1678"
        ///     }
        ///     Or
        ///     {
        ///        "StudentIdList": [],
        ///        "ClassId": "74b1eb4c-33ab-4882-9b6d-c0c6b4fd1678"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Return a Cart after modify success</response>
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
            var invalidStudentIds = cartRequest.StudentIdList.Except(students.Select(s => s.Id)).ToList();

            if (invalidStudentIds.Any())
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Invalid Student Id or you are trying to sign someone else's Student: [" 
                    + string.Join(", ", invalidStudentIds.Select(x => x.ToString()).ToList()) + "]",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }
            var result = await _cartService.ModifyCartOffCurrentParentAsync(cartRequest.StudentIdList, cartRequest.ClassId);
            return Ok(result);
        }

        #region document API get cart
        /// <summary>
        ///  View Cart of current Parent
        /// </summary>
        /// <response code="200">Show a Cart of current Parent</response>
        /// <response code="403">Invalid role</response>
        /// <response code="500">Unhandel database error</response>
        #endregion
        [HttpGet(ApiEndpointConstant.CartEnpoint.GetCart)]
        [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
        [Authorize(Roles = "PARENT")]
        public async Task<IActionResult> GetCart()
        {
            var cart = await _cartService.GetDetailCurrentParrentCart();
            return Ok(cart);
        }

        #region document API get favorite
        /// <summary>
        ///  View Favorite Course list of current Parent
        /// </summary>
        /// <response code="200">Show a cart of current parent</response>
        /// <response code="403">Invalid role</response>
        /// <response code="500">Unhandel database error</response>
        #endregion
        [HttpGet(ApiEndpointConstant.CartEnpoint.GetFavorite)]
        [ProducesResponseType(typeof(FavoriteResponse), StatusCodes.Status200OK)]
        [Authorize(Roles = "PARENT")]
        public async Task<IActionResult> GetFavorite()
        {
            var cart = await _cartService.GetDetailCurrentParrentFavorite();
            return Ok(cart);
        }

        #region document API delete item
        /// <summary>
        ///   Allow delete Single or Multiple item in cart, also delete Favorite item
        /// </summary>
        /// <param name="itemIdList">Id of all item want to delete </param>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///        "itemId": "77982AA8-5DFE-41AE-3776-08DBEB2BCC68"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Delete success</response>
        /// <response code="403">Invalid role</response>
        /// <response code="500">Unhandel database error</response>
        #endregion
        [HttpDelete(ApiEndpointConstant.CartEnpoint.DeleteCartItem)]
        [ProducesResponseType(typeof(String), StatusCodes.Status200OK)]
        [Authorize(Roles = "PARENT")]
        public async Task<IActionResult> DeleteCartItem([FromQuery] List<Guid> itemIdList)
        {
            await _cartService.DeleteItemInCartOfCurrentParentAsync(itemIdList);

            return Ok("Delete Success");
        }
    }
}
