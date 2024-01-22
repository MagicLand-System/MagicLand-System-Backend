using AutoMapper;
using AutoMapper.Internal;
using MagicLand_System.Config;
using MagicLand_System.Constants;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.Helpers;
using MagicLand_System.PayLoad.Request.Vnpay;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;

namespace MagicLand_System.Services.Implements
{
    public class GatewayService : BaseService<GatewayService>, IGatewayService
    {
        private readonly VnpayConfig _vnpayConfig;
        public GatewayService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<GatewayService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IOptions<VnpayConfig> vnpayConfig) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _vnpayConfig = vnpayConfig.Value;
        }

        public string GetLinkGateway(double amountMoney, string txnRefCode, string orderInfor)
        {
            var requestData = MakeRequestVnpayData(amountMoney, txnRefCode, orderInfor);

            StringBuilder data = new StringBuilder();

            foreach (KeyValuePair<string, string> kv in requestData)
            {
                if (!String.IsNullOrEmpty(kv.Value))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }

            string result = _vnpayConfig.PaymentUrl + "?" + data.ToString();
            var secureHash = StringHelper.HmacSHA512(_vnpayConfig.HashSecret, data.ToString().Remove(data.Length - 1, 1));

            return result += "vnp_SecureHash=" + secureHash;
        }

        public (string, bool) HandelReturnStatusVnpay(VnpayReturn vnpayReturn)
        {
            var responseCode = vnpayReturn.vnp_ResponseCode;
            var message = string.Empty;
            bool result = false;

            if (responseCode == null)
            {
                if (VnpayReturnCodeMessageConstant.GetCodeMessage.TryGetValue(-1, out message))
                {
                    return (message, false);
                }
            }

            if (!IsValidSignatureVnpay(vnpayReturn))
            {
                if (VnpayReturnCodeMessageConstant.GetCodeMessage.TryGetValue(-3, out message))
                {
                    return (message, false);
                }
            }

            message = HandelResponseCode(responseCode, ref result);

            return (message ?? string.Empty, result);
        }

        private static string? HandelResponseCode(string? responseCode, ref bool result)
        {
            string? message;
            switch (responseCode)
            {
                case "00":
                    VnpayReturnCodeMessageConstant.GetCodeMessage.TryGetValue(0, out message);
                    result = true;
                    break;

                case "09":
                    VnpayReturnCodeMessageConstant.GetCodeMessage.TryGetValue(9, out message);
                    break;

                case "10":
                    VnpayReturnCodeMessageConstant.GetCodeMessage.TryGetValue(10, out message);
                    break;

                case "11":
                    VnpayReturnCodeMessageConstant.GetCodeMessage.TryGetValue(11, out message);
                    break;

                case "13":
                    VnpayReturnCodeMessageConstant.GetCodeMessage.TryGetValue(13, out message);
                    break;

                case "24":
                    VnpayReturnCodeMessageConstant.GetCodeMessage.TryGetValue(24, out message);
                    break;
                case "65":
                    VnpayReturnCodeMessageConstant.GetCodeMessage.TryGetValue(65, out message);
                    break;
                default:
                    VnpayReturnCodeMessageConstant.GetCodeMessage.TryGetValue(-2, out message);
                    break;
            }

            return message;
        }

        private SortedList<string, string> MakeRequestVnpayData(double amount, string txnRefCode, string orderInfor)
        {
            var requestData = new SortedList<string, string>(new CompareHelper())
            {
                { "vnp_Amount", (amount * 100).ToString() },
                { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss")},
                { "vnp_ExpireDate", DateTime.Now.AddMinutes(5).ToString("yyyyMMddHHmmss")},
                { "vnp_Command", VnpayTransactionConstant.vnp_Command},
                { "vnp_CurrCode", VnpayTransactionConstant.vnp_CurrCode},
                { "vnp_Locale", VnpayTransactionConstant.vnp_Locale},
                { "vnp_OrderType", VnpayTransactionConstant.vnp_OrderType},
                { "vnp_Version", _vnpayConfig.Version},
                { "vnp_ReturnUrl", _vnpayConfig.ReturnUrl},
                { "vnp_TmnCode", _vnpayConfig.TmnCode},
                { "vnp_TxnRef", txnRefCode},
                { "vnp_OrderInfo", orderInfor},
            };

            var vnp_IpAddr = GetCurrentUserIpAdress();
            if (vnp_IpAddr != null)
                requestData.Add("vnp_IpAddr", vnp_IpAddr);

            return requestData;
        }
        private SortedList<string, string> MakeReturnVnpayData(VnpayReturn vnpReturn)
        {
            var returnData = new SortedList<string, string>(new CompareHelper());

            if (vnpReturn.vnp_Amount != null)
                returnData.Add("vnp_Amount", vnpReturn.vnp_Amount.ToString() ?? string.Empty);
            if (!string.IsNullOrEmpty(vnpReturn.vnp_TmnCode))
                returnData.Add("vnp_TmnCode", vnpReturn.vnp_TmnCode.ToString());
            if (!string.IsNullOrEmpty(vnpReturn.vnp_BankCode))
                returnData.Add("vnp_BankCode", vnpReturn.vnp_BankCode.ToString());
            if (!string.IsNullOrEmpty(vnpReturn.vnp_BankTranNo))
                returnData.Add("vnp_BankTranNo", vnpReturn.vnp_BankTranNo.ToString());
            if (!string.IsNullOrEmpty(vnpReturn.vnp_CardType))
                returnData.Add("vnp_CardType", vnpReturn.vnp_CardType.ToString());
            if (!string.IsNullOrEmpty(vnpReturn.vnp_OrderInfo))
                returnData.Add("vnp_OrderInfo", vnpReturn.vnp_OrderInfo.ToString());
            if (!string.IsNullOrEmpty(vnpReturn.vnp_TransactionNo))
                returnData.Add("vnp_TransactionNo", vnpReturn.vnp_TransactionNo.ToString());
            if (!string.IsNullOrEmpty(vnpReturn.vnp_TransactionStatus))
                returnData.Add("vnp_TransactionStatus", vnpReturn.vnp_TransactionStatus.ToString());
            if (!string.IsNullOrEmpty(vnpReturn.vnp_TxnRef))
                returnData.Add("vnp_TxnRef", vnpReturn.vnp_TxnRef.ToString());
            if (!string.IsNullOrEmpty(vnpReturn.vnp_PayDate))
                returnData.Add("vnp_PayDate", vnpReturn.vnp_PayDate.ToString());
            if (!string.IsNullOrEmpty(vnpReturn.vnp_ResponseCode))
                returnData.Add("vnp_ResponseCode", vnpReturn.vnp_ResponseCode);

            return returnData;
        }

        private bool IsValidSignatureVnpay(VnpayReturn vnpayReturn)
        {
            var returnData = MakeReturnVnpayData(vnpayReturn);
            StringBuilder data = new StringBuilder();

            foreach (KeyValuePair<string, string> kv in returnData)
            {
                if (!String.IsNullOrEmpty(kv.Value))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }
            string checkSum = StringHelper.HmacSHA512(_vnpayConfig.HashSecret, data.ToString().Remove(data.Length - 1, 1));

            return checkSum.Equals(vnpayReturn.vnp_SecureHash, StringComparison.InvariantCultureIgnoreCase);
        }

    }
}
