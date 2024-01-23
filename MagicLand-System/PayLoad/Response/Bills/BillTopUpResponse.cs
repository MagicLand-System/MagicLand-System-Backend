namespace MagicLand_System.PayLoad.Response.Bills
{
    public class BillTopUpResponse : BillResponse
    {
        public required string Customer { get; set; }
        public required string Currency { get; set; }

    }
}
