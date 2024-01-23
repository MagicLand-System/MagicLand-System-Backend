using MagicLand_System.Helpers;

namespace MagicLand_System.PayLoad.Request.Vnpay
{
    public class VnpayRequest
    {    
        public double? vnp_Amount { get; set; }
        public string? vnp_Command { get; set; }
        public DateTime vnp_CreateDate { get; set; } = DateTime.Now;
        public DateTime vnp_ExpireDate { get; set; } = DateTime.Now.AddMinutes(5);
        public string? vnp_CurrCode { get; set; }
        public string? vnp_IpAddr { get; set; }
        public string? vnp_Locale { get; set; }
        public string? vnp_OrderInfo { get; set; }
        public string? vnp_OrderType { get; set; }
        public string? vnp_ReturnUrl { get; set; }
        public string? vnp_TmnCode { get; set; }
        public string? vnp_TxnRef { get; set; }
        public string? vnp_Version { get; set; }
        public string? vnp_SecureHash { get; set; }
    }
}
