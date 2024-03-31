using MagicLand_System.Constants;
using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Request.Cart;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Carts;
using MagicLand_System.Services.Interfaces;
using MagicLand_System.Utils;
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
        /// Cho Phép Thêm Khóa Học Vào Danh Sách Quan Tâm
        /// </summary>
        /// <param name="courseId">Id Của Khóa Học </param>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///        "CourseId": "74b1eb4c-33ab-4882-9b6d-c0c6b4fd1678"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Quan Tâm Thành Công</response>
        /// <response code="400">Quan Tâm Thất Bại</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpPost(ApiEndpointConstant.CartEnpoint.AddCourseFavoriteList)]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        [ProducesResponseType(typeof(FavoriteResponse), StatusCodes.Status200OK)]
        [Authorize(Roles = "PARENT")]
        public async Task<IActionResult> AddCourseFavoriteList([FromQuery] Guid courseId)
        {

            if (await _courseService.GetCourseByIdAsync(courseId) == null)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Id [{courseId}] Khóa Học Không Tồn Tại",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }

            var result = await _cartService.AddCourseFavoriteOffCurrentParentAsync(courseId);
            if (result)
            {
                return Ok();
            }

            return BadRequest();
        }


        #region document API modify cart
        /// <summary>
        /// Cho Phép Thêm Lớp Có Hoặc Không Có Học Sinh Vào Giỏ Hàng
        /// </summary>
        /// <param name="cartRequest">Chứa Id Của Lớp Và Id Của Học Sinh Nếu Có</param>
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
        /// <response code="200">Thêm Thành Công Lớp Học Vào Giỏ Hàng</response>
        /// <response code="400">Thêm Thất Bại</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpPost(ApiEndpointConstant.CartEnpoint.ModifyCart)]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [Authorize(Roles = "PARENT")]
        public async Task<IActionResult> ModifyCart([FromBody] CartRequest cartRequest)
        {
            var cls = await _classService.GetClassByIdAsync(cartRequest.ClassId);
            if (cls == null)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Id [{cartRequest.ClassId}] Của Lớp Học Không Tồn Tại",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }
            if (cls.Status != ClassStatusEnum.UPCOMING.ToString())
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Chỉ Có Thể Thêm Các Lớp Học [Sắp Diễn Ra] Vào Giỏ Hàng, Lớp Này [{EnumUtil.CompareAndGetDescription<ClassStatusEnum>(cls.Status!)}]",
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
                    Error = $"Id Của Học Sinh Không Tồn Tại Hoặc Bạn Đang Sử Dụng Id Của Học Sinh Khác Không Phải Con Bạn ["
                    + string.Join(", ", invalidStudentIds.Select(x => x.ToString()).ToList()) + "]",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }

            var exsitStudents = students.Where(stu => cartRequest.StudentIdList.Any(id => id == stu.Id)).ToList();

            foreach (var student in exsitStudents)
            {
                if (!student.IsActive!.Value)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Error = $"Id [{student.Id}] Của Học Sinh Đã Ngưng Hoạt Động",
                        StatusCode = StatusCodes.Status400BadRequest,
                        TimeStamp = DateTime.Now,
                    });
                }
            }

            var result = await _cartService.ModifyCartOffCurrentParentAsync(cartRequest.StudentIdList, cartRequest.ClassId);
            if (result)
            {
                return Ok();
            }
            return BadRequest();
        }

        #region document API get cart
        /// <summary>
        ///  Truy Suất Giỏ Của Người Dùng Hiện Tại
        /// </summary>
        /// <response code="200">Trả Về Giỏ Hàng</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.CartEnpoint.GetCart)]
        [ProducesResponseType(typeof(WishListResponse), StatusCodes.Status200OK)]
        [Authorize(Roles = "PARENT")]
        public async Task<IActionResult> GetCart()
        {
            var cart = await _cartService.GetDetailCurrentParrentCart();
            return Ok(cart);
        }

        #region document API get favorite
        /// <summary>
        ///  Truy Suất Danh Sách Quan Tâm Của Nguời Dùng Hiện Tại
        /// </summary>
        /// <response code="200">Trả Về Danh Sách Quan Tâm</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
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
        ///   Cho Phép Xóa Một Hoặc Nhiều Item Trong Giỏ Hàng Hoặc Item Của Danh Sách Quan Tâm
        /// </summary>
        /// <param name="cartItemIdList">Id Của Tất Cả Item Cần Xóa </param>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///        "cartItemId": "77982AA8-5DFE-41AE-3776-08DBEB2BCC68"
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
        public async Task<IActionResult> DeleteCartItem([FromQuery] List<Guid> cartItemIdList)
        {
            await _cartService.DeleteItemInCartOfCurrentParentAsync(cartItemIdList);

            return Ok("Xóa Thành Công");
        }


        #region document API Get All Item In Cart
        /// <summary>
        ///  Truy Suất Toàn Bộ WishList Và FavoriteList ( items) Có Trong Giỏ Hàng Của Người Dùng Hiện Tại
        /// </summary>
        /// <response code="200">Trả Về Items Trong Giỏ Hàng</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.CartEnpoint.GetAll)]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        [ProducesResponseType(typeof(FavoriteResponse), StatusCodes.Status200OK)]
        [Authorize(Roles = "PARENT")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _cartService.GetAllItemsInCartAsync();

            return Ok(result);
        }
    }
}
