using MagicLand_System.Enums;

namespace MagicLand_System.PayLoad.Response.Cart
{
    public class BillResponse
    {
        public required string Status { get; set; }
        public required DateTime Date { get; set; }
        public required CheckOutMethodEnum Method { get; set; }
        public required string Payer { get; set; }
        public required double MoneyPaid { get; set; }
    }
}
