namespace MagicLand_System.PayLoad.Response.WalletTransactions
{
    public class RevenueResponse
    {
        public required int Number { get; set; }
        public required DateTime StartFrom { get; set; }
        public required DateTime EndAt { get; set; }
        public required double TotalMoneyEarn { get; set; }
        public required double TotalMoneyDiscount { get; set; }
        public required double TotalMoneyRefund { get; set; }
        public required double TotalRevenue { get; set; }
    }
}
