namespace MagicLand_System.PayLoad.Response.Vnpay
{
    public class GatewayResponseTxnCode : GatewayResponse
    {
        public required string TxnRefCode { get; set; }
    }
}
