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

        [HttpPost(ApiEndpointConstant.CartEnpoint.AddCart)]
        [Authorize(Roles = "PARENT")]
        public async Task<IActionResult> AddCart([FromBody] CartRequest cartRequest)
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

            var invalidStudentIds = cartRequest.StudentId.Except(students.Select(s => s.Id)).ToList();

            if (invalidStudentIds.Any())
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Invalid Student Id or you are trying to sign someone else's student: " + invalidStudentIds.Select(x => x.ToString()),
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }
            var result = await _cartService.AddCartAsync(cartRequest.StudentId, cartRequest.ClassId);
            return Ok(result);
        }


        [HttpGet(ApiEndpointConstant.CartEnpoint.GetCart)]
        [Authorize(Roles = "PARENT")]
        public async Task<IActionResult> GetCart()
        {
            var cart = await _cartService.GetCartOfCurrentParentAsync();
            return Ok(cart);
        }
    }
}
