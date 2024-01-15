using MagicLand_System.Constants;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MagicLand_System.Controllers
{
    [ApiController]
    public class WalletTransactionController : BaseController<WalletTransactionController>
    {
        private readonly IWalletTransactionService _walletTransactionService;
        public WalletTransactionController(ILogger<WalletTransactionController> logger,IWalletTransactionService walletTransactionService) : base(logger)
        {
            _walletTransactionService = walletTransactionService;
        }
        [HttpGet(ApiEndpointConstant.WalletTransaction.GetAll)]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(string? phone,DateTime? startDate,DateTime? endDate)
        {
            var result = await _walletTransactionService.GetWalletTransactions(phone,startDate,endDate);
            return Ok(result);
        }
        [HttpGet(ApiEndpointConstant.WalletTransaction.TransactionById)]
        [AllowAnonymous]
        public async Task<IActionResult> GetDetail(string id)
        {
            var result = await _walletTransactionService.GetWalletTransaction(id);
            return Ok(result);
        }
    }
}
