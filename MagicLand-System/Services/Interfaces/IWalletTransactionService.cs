using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Request.Cart;
using MagicLand_System.PayLoad.Request.Checkout;
using MagicLand_System.PayLoad.Response.Bills;
using MagicLand_System.PayLoad.Response.Students;
using MagicLand_System.PayLoad.Response.WalletTransactions;

namespace MagicLand_System.Services.Interfaces
{
    public interface IWalletTransactionService
    {
        Task<List<WalletTransactionResponse>> GetWalletTransactions(string phone = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<WalletTransactionResponse> GetWalletTransaction(string id);
        Task<BillTopUpResponse?> GenerateBillTransactionAsync(Guid id);
        Task<(Guid, string)> GenerateTopUpTransAsync(double money);
        Task<(string, double)> GeneratePaymentTransAsync(List<ItemGenerate> items);
        Task<bool> ValidRegisterAsync(List<StudentScheduleResponse> allStudentSchedules, Guid classId, List<Guid> studentIds);
        Task<BillPaymentResponse> CheckoutAsync(List<CheckoutRequest> requests);
        Task<(string, bool)> HandelSuccessReturnDataVnpayAsync(string transactionCode, string signature, TransactionTypeEnum type);
        Task<(string, bool)> HandelFailedReturnDataVnpayAsync(string transactionCode, string signature, TransactionTypeEnum type);
    }
}
