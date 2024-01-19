using MagicLand_System.PayLoad.Response.WalletTransactions;

namespace MagicLand_System.Services.Interfaces
{
    public interface IPersonalWalletService
    {
        Task<WalletResponse> GetWalletOfCurrentUser();
    }
}
