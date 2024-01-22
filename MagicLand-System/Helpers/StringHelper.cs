using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Request.Checkout;
using System.Security.Cryptography;
using System.Text;

namespace MagicLand_System.Helpers
{
    public class StringHelper
    {
        public static string TrimStringAndNoSpace(string input)
        {
            return input.Trim().Replace(" ", "");
        }

        public static string GenerateTransactionCode(TransactionTypeEnum type)
        {
            const string chars = "0123456789";
            Random random = new Random();

            string randomCode = "";

            switch (type)
            {
                case TransactionTypeEnum.Refund:
                    randomCode += "10";
                    break;

                case TransactionTypeEnum.Payment:
                    randomCode += "11";
                    break;

                case TransactionTypeEnum.TopUp:
                    randomCode += "12";
                    break;
            }
            randomCode += new string(Enumerable.Repeat(chars, 14).Select(s => s[random.Next(s.Length)]).ToArray());

            return randomCode;
        }
        public static string GenerateTransactionTxnRefCode(TransactionTypeEnum type, string valueAttach)
        {

            string txnRefCode = string.Empty;

            switch (type)
            {
                case TransactionTypeEnum.Refund:
                    txnRefCode = "RF"  + valueAttach;
                    break;

                case TransactionTypeEnum.Payment:
                    txnRefCode = "PM"  + valueAttach;
                    break;

                case TransactionTypeEnum.TopUp:
                    txnRefCode = "TU" + valueAttach;
                    break;
            }

            return txnRefCode;
        }

        public static string GenerateValueAttachForPayment(List<CheckoutRequest> requests, List<Guid> cartItems)
        {
            var listValue = new List<string>();
            foreach (var request in requests)
            {
                listValue.Add($"[{TransactionAttachValueEnum.ClassId}:{request.ClassId}][{TransactionAttachValueEnum.StudentId}:{string.Join(", ", request.StudentIdList)}]");
            }
            if(cartItems != null && cartItems.Count > 0)
            {
                listValue.Add($"[{TransactionAttachValueEnum.CartItemId}:{string.Join(", ", cartItems)}]");
            }

            return string.Join(" | ", listValue) + GenerateUniqueOrderCode(requests.Count());
        }

        private static string GenerateUniqueOrderCode(int numberItem)
        {
            if (numberItem == 1)
            {
                return string.Empty;
            }

            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            Random random = new Random();
            return "?UniqueCode=" + new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GetAttachValueFromTxnRefCode(string txnRefCode)
        {
            string value = DecodeString(txnRefCode);
            return value.Substring(7);
        }


        public static string EncodeString(string input)
        {
            byte[] stringBytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(stringBytes);
        }

        public static string DecodeString(string input)
        {
            byte[] stringBytes = Convert.FromBase64String(input);
            return Encoding.UTF8.GetString(stringBytes);
        }

        public static string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hasValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hasValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }

            return hash.ToString();
        }
    }
}
