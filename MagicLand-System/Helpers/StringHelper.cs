using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Request.Cart;
using MagicLand_System.PayLoad.Request.Checkout;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

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
        public static string GenerateTransactionTxnRefCode(TransactionTypeEnum type)
        {
            string txnRefCode = string.Empty;
            string uniqueId = Guid.NewGuid().ToString("N");

            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();

            StringBuilder interleavedString = new StringBuilder();
            int minLength = Math.Min(uniqueId.Length, chars.Length);

            for (int i = 0; i < minLength; i++)
            {
                interleavedString.Append(uniqueId[i]);
                interleavedString.Append(chars[i]);
            }

            interleavedString.Append(uniqueId.Substring(minLength));
            interleavedString.Append(chars.Substring(minLength));

            string shuffledString = new string(interleavedString.ToString().ToCharArray().OrderBy(c => random.Next()).ToArray());

            string extraPart = shuffledString.Substring(0, Math.Min(5, shuffledString.Length));

            switch (type)
            {
                case TransactionTypeEnum.Refund:
                    txnRefCode = TransactionTypeCodeEnum.RF.ToString() + new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray()) + extraPart;
                    break;

                case TransactionTypeEnum.Payment:
                    txnRefCode = TransactionTypeCodeEnum.PM.ToString() + new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray()) + extraPart;
                    break;

                case TransactionTypeEnum.TopUp:
                    txnRefCode = TransactionTypeCodeEnum.TU + new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray()) + extraPart;
                    break;
            }

            return txnRefCode;
        }

        public static string GenerateAttachValueForTxnRefCode(ItemGenerate item)
        {
            string attachValue = $"[{TransactionAttachValueEnum.ClassId}:{item.ClassId}][{TransactionAttachValueEnum.StudentId}:{string.Join(", ", item.StudentIdList)}]";

            if (item.CartItemId != default)
            {
                attachValue += $"[{TransactionAttachValueEnum.CartItemId}:{item.CartItemId}]";
            }

            return EncodeAttachValue(attachValue);
        }

        public static Dictionary<string, List<string>> ExtractAttachValueFromSignature(string signature)
        {
            string attachValue = DecodeAttachValue(signature.Substring(12));

            Dictionary<string, List<string>> values = new Dictionary<string, List<string>>();
            Regex pattern = new Regex(@"\[(\w+):([^[\]]+)\]");

            MatchCollection matches = pattern.Matches(attachValue);
            foreach (Match match in matches)
            {
                string key = match.Groups[1].Value;
                string[] rawValues = match.Groups[2].Value.Split(',').Select(v => v.Trim()).ToArray();

                if (!values.ContainsKey(key))
                {
                    values[key] = new List<string>();
                }

                values[key].AddRange(rawValues);
            }

            return values;
        }
        private static string EncodeAttachValue(string input)
        {
            byte[] stringBytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(stringBytes);
        }

        public static string DecodeAttachValue(string input)
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
