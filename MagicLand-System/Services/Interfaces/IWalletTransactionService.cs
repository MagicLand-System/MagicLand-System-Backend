using MagicLand_System.PayLoad.Response.WalletTransactions;

namespace MagicLand_System.Services.Interfaces
{
    public interface IWalletTransactionService
    {
        Task<List<WalletTransactionResponse>> GetWalletTransactions(string phone = null , DateTime? startDate = null , DateTime? endDate = null);
        Task<WalletTransactionResponse> GetWalletTransaction(string id);
    }
}
