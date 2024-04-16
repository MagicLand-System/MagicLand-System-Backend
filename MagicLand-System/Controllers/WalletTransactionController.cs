using MagicLand_System.Constants;
using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Response.Bills;
using MagicLand_System.PayLoad.Response.WalletTransactions;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MagicLand_System.Controllers
{
    [ApiController]
    public class WalletTransactionController : BaseController<WalletTransactionController>
    {
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly IPersonalWalletService _personalWalletService;
        public WalletTransactionController(ILogger<WalletTransactionController> logger, IWalletTransactionService walletTransactionService, IPersonalWalletService personalWalletService) : base(logger)
        {
            _walletTransactionService = walletTransactionService;
            _personalWalletService = personalWalletService;
        }
        [HttpGet(ApiEndpointConstant.WalletTransactionEndPoint.GetAll)]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(string? phone, DateTime? startDate, DateTime? endDate, string? transactionCode)
        {
            var result = await _walletTransactionService.GetWalletTransactions(phone, startDate, endDate, transactionCode);
            return Ok(result);
        }
        [HttpGet(ApiEndpointConstant.WalletTransactionEndPoint.TransactionById)]
        [AllowAnonymous]
        public async Task<IActionResult> GetDetail(string id)
        {
            var result = await _walletTransactionService.GetWalletTransaction(id);
            return Ok(result);
        }
        [HttpGet(ApiEndpointConstant.WalletTransactionEndPoint.PersonalWallet)]
        [Authorize]
        public async Task<IActionResult> GetWallet()
        {
            var result = await _personalWalletService.GetWalletOfCurrentUser();
            return Ok(result);
        }


        #region document API Get Transaction By Id
        /// <summary>
        ///  Trả Về Hóa Đơn Của Một Đơn Hàng Thông Qua Id
        /// </summary>
        /// <param name="id">Id Của Đơn Hàng</param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///    "id":"3c1849af-400c-43ca-979e-58c71ce9301d"
        ///}
        /// </remarks>
        /// <response code="200">Trả Về Hóa Đơn | Trả Về Rỗng Khi Đơn Hàng Đang Sử Lý</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpPost(ApiEndpointConstant.WalletTransactionEndPoint.GetBillTransactionById)]
        [ProducesResponseType(typeof(BillPaymentResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequest))]
        [AllowAnonymous]
        public async Task<IActionResult> GetBillTransactionById([FromRoute] Guid id)
        {
            var response = await _walletTransactionService.GenerateBillTopUpTransactionAsync(id);
            if (response == default)
            {
                return Ok();
            }
            return Ok(response);
        }

        #region document API Get Transaction By TxnRefCode
        /// <summary>
        ///  Trả Về Hóa Đơn Các Đơn Hàng Thuộc TxnRefCode
        /// </summary>
        /// <param name="txnRefCode">TxnRefCode</param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///    "txnRefCode":"PM7sikpS9IfeEuPgdv95T07OcR554GnoAhJsW"
        ///}
        /// </remarks>
        /// <response code="200">Trả Về Hóa Đơn | Trả Về Rỗng Khi Đơn Hàng Đang Sử Lý</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpPost(ApiEndpointConstant.WalletTransactionEndPoint.GetBillTransactionByTxnRefCode)]
        [ProducesResponseType(typeof(BillPaymentResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequest))]
        [AllowAnonymous]
        public async Task<IActionResult> GetBillTransactionById([FromRoute] string txnRefCode)
        {
            var response = await _walletTransactionService.GenerateBillPaymentTransactionAssync(txnRefCode);
            if (response == default)
            {
                return Ok();
            }
            return Ok(response);
        }

        #region document API Get Total Revenue
        /// <summary>
        ///  Truy Suất Tổng Kết Doanh Thu Theo Thời Gian
        /// </summary>
        /// <param name="time">Truy Suất Tổng Doanh Thu Sắp Xếp Theo Thời Gian Đã Chọn, Mặc Định Là Theo Tuần</param>
        /// <response code="200">Trả Về Danh Sách Doanh Thu Theo Thời Gian | Trả Về Rỗng Khi Không Có Giao Dịch</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.WalletTransactionEndPoint.GetRevenueTransactionByTime)]
        [ProducesResponseType(typeof(RevenueResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequest))]
        [AllowAnonymous]
        public async Task<IActionResult> GetRevenueTransactionByTime([FromQuery] RevenueTimeEnum time)
        {
            var response = await _walletTransactionService.GetRevenueTransactionByTimeAsync(time);
            return Ok(response);
        }
        [HttpPost(ApiEndpointConstant.WalletTransactionEndPoint.CheckoutByStaff)]
        [ProducesResponseType(typeof(BillPaymentResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequest))]
        [AllowAnonymous]
        public async Task<IActionResult> CheckoutByStaff(StaffCheckoutRequest request)
        {
            var response = await _walletTransactionService.CheckoutByStaff(request);    
            return Ok(response);
        }

    }
}
