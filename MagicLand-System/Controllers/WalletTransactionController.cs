using MagicLand_System.Config;
using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Request.Vnpay;
using MagicLand_System.PayLoad.Response.Vnpay;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.Services.Implements;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using MagicLand_System.PayLoad.Response.Bills;

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
        [HttpGet(ApiEndpointConstant.WalletTransaction.GetAll)]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(string? phone, DateTime? startDate, DateTime? endDate)
        {
            var result = await _walletTransactionService.GetWalletTransactions(phone, startDate, endDate);
            return Ok(result);
        }
        [HttpGet(ApiEndpointConstant.WalletTransaction.TransactionById)]
        [AllowAnonymous]
        public async Task<IActionResult> GetDetail(string id)
        {
            var result = await _walletTransactionService.GetWalletTransaction(id);
            return Ok(result);
        }
        [HttpGet(ApiEndpointConstant.WalletTransaction.PersonalWallet)]
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
        [HttpPost(ApiEndpointConstant.WalletTransaction.GetBillTransactionById)]
        [ProducesResponseType(typeof(BillPaymentResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequest))]
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
        [HttpPost(ApiEndpointConstant.WalletTransaction.GetBillTransactionByTxnRefCode)]
        [ProducesResponseType(typeof(BillPaymentResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequest))]
        public async Task<IActionResult> GetBillTransactionById([FromRoute] string txnRefCode)
        {
            var response = await _walletTransactionService.GenerateBillPaymentTransactionAssync(txnRefCode);
            if (response == default)
            {
                return Ok();
            }
            return Ok(response);
        }
    }
}
