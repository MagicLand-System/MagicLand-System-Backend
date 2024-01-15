using MagicLand_System.Enums;

namespace MagicLand_System.PayLoad.Response.Carts
{
    public class BillResponse
    {
        public required string Status { get; set; }
        public required string Message { get; set; }
        public required double Cost { get; set; }
        public required double Discount { get; set; }
        public required double MoneyPaid { get; set; }
        public required DateTime Date { get; set; }
        public required string Method { get; set; }
        public required string Payer { get; set; }
       
    }
}
