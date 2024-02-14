using MagicLand_System.Config;
using MagicLand_System.Constants;
using MagicLand_System.Enums;
using MagicLand_System.Helpers;
using MagicLand_System.PayLoad.Request.Vnpay;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Vnpay;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MagicLand_System.Controllers
{
    [ApiController]
    public class WalletController : BaseController<WalletController>
    {
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly IGatewayService _gatewayService;

        public WalletController(ILogger<WalletController> logger, IWalletTransactionService walletTransactionService, IGatewayService gatewayService) : base(logger)
        {
            _walletTransactionService = walletTransactionService;
            _gatewayService = gatewayService;
        }

        #region document API Top Up Wallet
        /// <summary>
        ///  Nạp Tiền Vào Ví Hệ Thống Thông Qua Cổng Thanh Toán Vnpay
        /// </summary>
        /// <param name="amountMoney">Số Tiền Cần Nạp</param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///    "amountMoney":"10000"
        ///}
        /// </remarks>
        /// <response code="200">Trả Về Id Thanh Toán Và Url Của Cổng Thanh Toán</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpPost(ApiEndpointConstant.WalletEndPoint.TopUpWallet)]
        [ProducesResponseType(typeof(GatewayResponseTransactionId), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequestObjectResult))]
        [Authorize(Roles = "PARENT")]
        public async Task<IActionResult> TopUpWallet([FromQuery] double amountMoney)
        {
            if (amountMoney <= 0 || amountMoney < 100000 || amountMoney > 5000000)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Số Tiền Không Hợp Lệ, Mỗi Giao Dịch Nạp Tiền Chỉ Nhận Giao Dịch Từ [10.0000 VND] Đến [5.000.000 VND]",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }
            var transResult = await _walletTransactionService.GenerateTopUpTransAsync(amountMoney);
            var linkResult = _gatewayService.GetLinkGateway(amountMoney, transResult.Item2, "Top Up System Wallet");

            var response = new GatewayResponseTransactionId
            {
                TransactionId = transResult.Item1,
                PaymentGatewayUrl = linkResult,
            };

            return Ok(response);
        }

        #region document API Call Back From Transaction Gateway
        /// <summary>
        ///  Sử Lý Phản Hồi Từ Cổng Thanh Toán
        /// </summary>
        #endregion
        [HttpGet]
        [Route("sytem/handler/payment/return")]
        public async Task<IActionResult> CallBackFromTransGateway([FromQuery] VnpayReturn request)
        {

            var checkingStatus = _gatewayService.HandelReturnStatusVnpay(request);

            string message = checkingStatus.Item1;
            var type = request.vnp_TxnRef.Substring(0, Math.Min(2, request.vnp_TxnRef.Length)) == TransactionTypeCodeEnum.TU.ToString()
                ? TransactionTypeEnum.TopUp
                : request.vnp_TxnRef.Substring(0, Math.Min(2, request.vnp_TxnRef.Length)) == TransactionTypeCodeEnum.PM.ToString()
                ? TransactionTypeEnum.Payment
                : TransactionTypeEnum.Refund;

            if (checkingStatus.Item2)
            {
                var response = await _walletTransactionService.HandelSuccessReturnDataVnpayAsync(request.vnp_TransactionNo, request.vnp_TxnRef, request.vnp_BankCode, type);
                if (!response.Item2)
                {
                    message = response.Item1;
                }
            }
            else
            {
                var response = await _walletTransactionService.HandelFailedReturnDataVnpayAsync(request.vnp_TransactionNo, request.vnp_TxnRef, request.vnp_BankCode, type);
                if (!response.Item2)
                {
                    message += ", " + response.Item1;
                }
            }

            return Ok(message);
        }
    }
}
