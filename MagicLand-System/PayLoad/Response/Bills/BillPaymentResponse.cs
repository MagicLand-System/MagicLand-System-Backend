using MagicLand_System.Enums;

namespace MagicLand_System.PayLoad.Response.Bills
{
    public class BillPaymentResponse : BillResponse
    {
        public required double Discount { get; set; }
        public required double MoneyPaid { get; set; }
        public required string Payer { get; set; }
    }
}
