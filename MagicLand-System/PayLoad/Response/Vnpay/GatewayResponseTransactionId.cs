namespace MagicLand_System.PayLoad.Response.Vnpay
{
    public class GatewayResponseTransactionId : GatewayResponse
    {
        public required Guid TransactionId { get; set; }
    }
}
