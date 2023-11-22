using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Cart;
using MagicLand_System.PayLoad.Response.Class;
using MagicLand_System.Services.Implements;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

        #region document API add new item to cart
        /// <summary>
        /// Add class with student, which parent are interested, in to cart
        /// </summary>
        /// <param name="studentIds">Id of all student that parent register to class </param>
        /// <param name="classId">Id of class that parent registered</param>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///        "studentIds": "172c40fe-32e4-43fd-b982-c87afe8b54fa", "f9113f7e-ae51-4f65-a7b4-2348f666787d"
        ///        "classId": "74b1eb4c-33ab-4882-9b6d-c0c6b4fd1678"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Add cart success</response>
        /// <response code="400">Request invalid</response>
        /// <response code="403">Invalid role</response>
        /// <response code="500">Unhandel database error</response>
        #endregion
        [HttpGet(ApiEndpointConstant.CartEnpoint.AddCart)]
        [ProducesResponseType(typeof(String), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnauthorizedAccessException), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(UnhandledExceptionEventHandler), StatusCodes.Status500InternalServerError)]
        //[ProducesErrorResponseType(typeof(ErrorResponse))]
        [ProducesErrorResponseType(typeof(BadHttpRequestException))]
        [Authorize(Roles = "PARENT")]
        public async Task<IActionResult> AddCart([FromQuery] List<Guid> studentIds, [FromQuery] Guid classId)
        {
            if (await _classService.GetClassByIdAsync(classId) == null)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Class Id Not Esxit",
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
                    Error = "Invalid Student Id or you are trying to sign someone else's student: " + invalidStudentIds.Select(x => x.ToString()),
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }
            var result = await _cartService.AddCartAsync(studentIds, classId);
            return Ok(result);
        }



        #region document API get cart
        /// <summary>
        ///  View Cart Of Current Parent
        /// </summary>
        /// <response code="200">Show a cart of current parent</response>
        /// <response code="403">Invalid role</response>
        /// <response code="500">Unhandel database error</response>
        #endregion
        [HttpGet(ApiEndpointConstant.CartEnpoint.GetCart)]
        [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnauthorizedAccessException), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(UnhandledExceptionEventHandler), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "PARENT")]
        public async Task<IActionResult> GetCart()
        {
            var cart = await _cartService.GetCartOfCurrentParentAsync();
            return Ok(cart);
        }
    }
}
