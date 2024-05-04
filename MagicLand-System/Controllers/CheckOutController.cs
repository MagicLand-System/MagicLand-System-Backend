using Azure.Core;
using MagicLand_System.Constants;
using MagicLand_System.Helpers;
using MagicLand_System.PayLoad.Request.Cart;
using MagicLand_System.PayLoad.Request.Checkout;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Bills;
using MagicLand_System.PayLoad.Response.Carts;
using MagicLand_System.PayLoad.Response.Students;
using MagicLand_System.PayLoad.Response.Vnpay;
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
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly IGatewayService _gatewayService;
        public CheckOutController(ILogger<CheckOutController> logger, IUserService userService, ICartService cartService, IClassService classService, IStudentService studentService, IWalletTransactionService walletTransactionService, IGatewayService gatewayService) : base(logger)
        {
            _userService = userService;
            _cartService = cartService;
            _classService = classService;
            _studentService = studentService;
            _walletTransactionService = walletTransactionService;
            _gatewayService = gatewayService;
        }

        #region document API Check-out Class
        /// <summary>
        /// Thanh Toán Một Hoặc Nhiều Lớp
        /// </summary>
        /// <param name="requests">Chứa Id Của Các Lớp Muốn Thanh Toán Và Id Của Các Học Sinh Trong Đó</param>
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
        /// <response code="200">Trả Về Hóa Đơn Sau Khi Thanh Toán</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpPost(ApiEndpointConstant.UserEndpoint.CheckoutClass)]
        [ProducesResponseType(typeof(BillPaymentResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequestObjectResult))]
        [Authorize(Roles = "PARENT")]
        public async Task<IActionResult> CheckoutClass(List<CheckoutRequest> requests)
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

                if (!await _walletTransactionService.ValidRegisterAsync(allStudentSchedules, request.ClassId, request.StudentIdList))
                {
                    return BadRequest(new ErrorResponse
                    {
                        Error = "Yêu Cầu Vi Phạm Một Số Tiêu Chuẩn Lớp Học",
                        StatusCode = StatusCodes.Status500InternalServerError,
                        TimeStamp = DateTime.Now,
                    });
                }
            }
            await _classService.ValidateScheduleAmongClassesAsync(requests.Select(r => r.ClassId).ToList());

            var response = await _walletTransactionService.CheckoutAsync(requests);

            return Ok(response);
        }

        #region document API Check-out Class By Vnpay
        /// <summary>
        /// Thanh Toán Một Hoặc Nhiều Lớp Qua Cổng Thanh Toán Vnpay
        /// </summary>
        /// <param name="requests">Chứa Id Của Các Lớp Muốn Thanh Toán Và Id Của Các Học Sinh Trong Đó</param>
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
        /// <response code="200">Trả Về Mã Giao Dịch Chung Và Url Gateway</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpPost(ApiEndpointConstant.UserEndpoint.CheckOutClassByVnpay)]
        [ProducesResponseType(typeof(BillPaymentResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequestObjectResult))]
        [Authorize(Roles = "PARENT")]
        public async Task<IActionResult> CheckOutClassByVnpay(List<CheckoutRequest> requests)
        {
            var itemGenerates = new List<ItemGenerate>();

            foreach (var request in requests)
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

                if (!await _walletTransactionService.ValidRegisterAsync(allStudentSchedules, request.ClassId, request.StudentIdList))
                {
                    return BadRequest(new ErrorResponse
                    {
                        Error = "Yêu Cầu Vi Phạm Một Số Tiêu Chuẩn Lớp Học",
                        StatusCode = StatusCodes.Status500InternalServerError,
                        TimeStamp = DateTime.Now,
                    });
                }

                itemGenerates.Add(new ItemGenerate
                {
                    ClassId = request.ClassId,
                    StudentIdList = request.StudentIdList,
                });
            }
            await _classService.ValidateScheduleAmongClassesAsync(requests.Select(r => r.ClassId).ToList());

            var transResult = await _walletTransactionService.GeneratePaymentTransAsync(itemGenerates);
            var linkResult = _gatewayService.GetLinkGateway(transResult.Item2, transResult.Item1, "Register Students Into Classes");

            var response = new GatewayResponseTxnCode
            {
                TxnRefCode = transResult.Item1,
                PaymentGatewayUrl = linkResult,
            };

            return Ok(response);
        }



        #region document API Check-out Cart Vnpay
        /// <summary>
        ///  Cho Phép Thanh Toán Toàn Bộ Item Được Chọn Trong Giỏ Hàng Qua Cổng Giao Dịch Vnpay
        /// </summary>
        /// <param name="cartItemIdList">Id Của Tất Cả Item Muốn Thanh Toán</param>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///       "CartItemIdList" : {"d3407e14-c7fc-49ff-ade3-438bedf415a8", "g3d07e14-ccrc-49ff-ade3-438bedolpkms"}
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Trả Về Mã Giao Dịch Chung Và Url Gateway</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpPost(ApiEndpointConstant.CartEndpoint.CheckOutCartItemByVnpay)]
        [ProducesResponseType(typeof(WishListResponse), StatusCodes.Status200OK)]
        [Authorize(Roles = "PARENT")]
        public async Task<IActionResult> CheckOutCartItemByVnpay([FromBody] List<Guid> cartItemIdList)
        {

            var result = await ValidCartItem(cartItemIdList);
            var items = result as OkObjectResult;

            if (items == null)
            {
                return result;
            }

            var itemGenerates = new List<ItemGenerate>();

            foreach (var item in (List<WishListItemResponse>)items.Value!)
            {
                var allStudentSchedules = new List<StudentScheduleResponse>();

                foreach (var task in item!.Students.Select(async stu => await _studentService
                .GetScheduleOfStudent(stu.StudentId.ToString())))
                {
                    var schedules = await task;
                    allStudentSchedules.AddRange(schedules);

                }

                if (!await _walletTransactionService.ValidRegisterAsync(allStudentSchedules, item.Class.ClassId, item.Students.Select(stu => stu.StudentId).ToList()))
                {
                    return BadRequest(new ErrorResponse
                    {
                        Error = "Yêu Cầu Vi Phạm Một Số Tiêu Chuẩn Lớp Học",
                        StatusCode = StatusCodes.Status500InternalServerError,
                        TimeStamp = DateTime.Now,
                    });
                }

                itemGenerates.Add(new ItemGenerate
                {
                    CartItemId = item.CartItemId,
                    ClassId = item.Class.ClassId,
                    StudentIdList = item.Students.Select(stu => stu.StudentId).ToList(),
                });

            }
            await _classService.ValidateScheduleAmongClassesAsync(itemGenerates.Select(ig => ig.ClassId).ToList());

            var transResult = await _walletTransactionService.GeneratePaymentTransAsync(itemGenerates);
            var linkResult = _gatewayService.GetLinkGateway(transResult.Item2, transResult.Item1, "Register Students Into Classes From Cart");

            var response = new GatewayResponseTxnCode
            {
                TxnRefCode = transResult.Item1,
                PaymentGatewayUrl = linkResult,
            };

            return Ok(response);
        }


        #region document API check-out cart
        /// <summary>
        ///  Cho Phép Thanh Toán Toàn Bộ Item Được Chọn Trong Giỏ Hàng
        /// </summary>
        /// <param name="cartItemIdList">Id Của Tất Cả Item Muốn Thanh Toán</param>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///       "CartItemIdList" : {"d3407e14-c7fc-49ff-ade3-438bedf415a8", "g3d07e14-ccrc-49ff-ade3-438bedolpkms"}
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Trả Về Hóa Đơn Sau Khi Thanh Toán</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpPost(ApiEndpointConstant.CartEndpoint.CheckOutCartItem)]
        [ProducesResponseType(typeof(WishListResponse), StatusCodes.Status200OK)]
        [Authorize(Roles = "PARENT")]
        public async Task<IActionResult> CheckOutCartItem([FromBody] List<Guid> cartItemIdList)
        {
            var result = await ValidCartItem(cartItemIdList);
            var items = result as OkObjectResult;

            if (items == null)
            {
                return result;
            }

            var requests = new List<CheckoutRequest>();

            foreach (var item in (List<WishListItemResponse>)items.Value!)
            {
                var allStudentSchedules = new List<StudentScheduleResponse>();

                foreach (var task in item!.Students.Select(async stu => await _studentService
                .GetScheduleOfStudent(stu.StudentId.ToString())))
                {
                    var schedules = await task;
                    allStudentSchedules.AddRange(schedules);

                }

                if (!await _walletTransactionService.ValidRegisterAsync(allStudentSchedules, item.Class.ClassId, item.Students.Select(stu => stu.StudentId).ToList()))
                {
                    return BadRequest(new ErrorResponse
                    {
                        Error = "Yêu Cầu Vi Phạm Một Số Tiêu Chuẩn Lớp Học",
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
            await _classService.ValidateScheduleAmongClassesAsync(requests.Select(r => r.ClassId).ToList());
            var response = await _walletTransactionService.CheckoutAsync(requests);
            await _cartService.DeleteItemInCartOfCurrentParentAsync(cartItemIdList);

            return Ok(response);
        }


        private async Task<IActionResult> ValidCartItem(List<Guid> cartItemIds)
        {
            var cart = await _cartService.GetDetailCurrentParrentCart();

            var invalidItem = cartItemIds.Except(cart.Items.Select(s => s.CartItemId)).ToList();
            if (invalidItem.Any())
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Id [{string.Join(", ", invalidItem.ToArray())}] Của Item Không Tồn Tại Trong Giỏ Hàng",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }

            var items = cartItemIds.Select(ci => cart.Items.Single(c => c.CartItemId == ci)).ToList();

            var emptyStudentItem = items.Where(x => x.Students.Count() == 0 || x.Class == null).ToList();
            if (emptyStudentItem.Count() > 0)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Có Một Hoặc Nhiều Item Của Giỏ Hàng Không Có Lớp Hoặc Học Sinh Đăng Ký " +
                    $"[{string.Join(", ", emptyStudentItem.Select(x => x.CartItemId).ToList())}]",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }

            return Ok(items);
        }
        private async Task<IActionResult> ValidRequest(Guid classId, List<Guid> studentIds)
        {

            if (await _classService.GetClassByIdAsync(classId) == default)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Id [{classId}] Của Lớp Học Không Tồn Tại",
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
                    Error = "Id Của Học Sinh Không Tồn Tại Hoặc Bạn Đăng Sử Dụng Id Của Học Sinh Khác Không Phải Con Bạn " +
                    $"[{string.Join(", ", invalidStudentIds.Select(x => x.ToString()))}]",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }

            var exsitStudents = students.Where(stu => studentIds.Any(id => id == stu.Id)).ToList();

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

            Guid duplicateStudentId = studentIds.GroupBy(x => x)
               .Where(list => list.Count() > 1)
               .Select(list => list.First())
               .SingleOrDefault();


            if (duplicateStudentId != default)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Bạn Đang Đăng Ký Cho Học Sinh [{students.Where(x => x.Id == duplicateStudentId).Single().FullName}] " +
                    "Nhiều Hơn Hai Lần Vào Một Lớp",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }

            return Ok();
        }
    }
}
