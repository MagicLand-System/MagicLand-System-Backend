namespace MagicLand_System.PayLoad.Response.Bills
{
    public class BillRefundResponse : BillResponse
    {
        public required double TotalDiscountOfItem { get; set; }
        public required double TotalRefund { get; set; }
        public required int NumberItemRefund { get; set; }
        public required string RefundToWallet { get; set; }
    }
}
