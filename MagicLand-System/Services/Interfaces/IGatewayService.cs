using MagicLand_System.PayLoad.Request.Vnpay;

namespace MagicLand_System.Services.Interfaces
{
    public interface IGatewayService
    {
        string GetLinkGateway(double amountMoney, string txnRefCode, string orderInfor);
        (string, bool) HandelReturnStatusVnpay(VnpayReturn vnpayReturn);
    }
}
