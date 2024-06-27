namespace MagicLand_System.PayLoad.Response.Bills
{
    public class BillResponse
    {
        public required string Status { get; set; }
        public required string Message { get; set; }
        public required double MoneyAmount { get; set; }
        public required DateTime Date { get; set; }
        public required string Method { get; set; }
        public required string Type { get; set; }
    }
}
