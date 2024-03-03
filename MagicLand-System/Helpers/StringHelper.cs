using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Request.Cart;
using MagicLand_System.Utils;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace MagicLand_System.Helpers
{
    public class StringHelper
    {
        public static string GetSlotNumber(string startTime)
        {
            if (startTime == EnumUtil.GetDescriptionFromEnum(SlotEnum.Slot1).Trim())
            {
                return "Slot 1";
            }
            if (startTime == EnumUtil.GetDescriptionFromEnum(SlotEnum.Slot2).Trim())
            {
                return "Slot 2";
            }
            if (startTime == EnumUtil.GetDescriptionFromEnum(SlotEnum.Slot3).Trim())
            {
                return "Slot 3";
            }
            if (startTime == EnumUtil.GetDescriptionFromEnum(SlotEnum.Slot4).Trim())
            {
                return "Slot 4";
            }
            if (startTime == EnumUtil.GetDescriptionFromEnum(SlotEnum.Slot5).Trim())
            {
                return "Slot 5";
            }
            if (startTime == EnumUtil.GetDescriptionFromEnum(SlotEnum.Slot6).Trim())
            {
                return "Slot 6";
            }

            return "Undefined";
        }

        public static (string, string) SplitStringByNewLine(string input)
        {
            string[] parts = input.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 2)
            {
                return (TrimStringAndNoSpace(parts[0]), TrimStringAndNoSpace(parts[1]));
            }
            else if (parts.Length == 1)
            {
                return (TrimStringAndNoSpace(parts[0]), string.Empty);
            }
            else
            {
                return (string.Empty, string.Empty);
            }
        }


        public static List<string> FromStringToList(string input)
        {
            var separators = new string[] { "/r/n", "\r\n", "\n" };

            var result = input.Split(separators, StringSplitOptions.RemoveEmptyEntries)
                              .Select(line => line.Trim())
                              .ToList();

            return result;
        }

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
            randomCode += new string(Enumerable.Repeat(chars, 11).Select(s => s[random.Next(s.Length)]).ToArray());

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

            string extraPart = shuffledString.Substring(0, Math.Min(30, shuffledString.Length));

            switch (type)
            {
                case TransactionTypeEnum.Refund:
                    txnRefCode = TransactionTypeCodeEnum.RF.ToString() + new string(Enumerable.Repeat(chars, 4).Select(s => s[random.Next(s.Length)]).ToArray()) + extraPart;
                    break;

                case TransactionTypeEnum.Payment:
                    txnRefCode = TransactionTypeCodeEnum.PM.ToString() + new string(Enumerable.Repeat(chars, 4).Select(s => s[random.Next(s.Length)]).ToArray()) + extraPart;
                    break;

                case TransactionTypeEnum.TopUp:
                    txnRefCode = TransactionTypeCodeEnum.TU + new string(Enumerable.Repeat(chars, 4).Select(s => s[random.Next(s.Length)]).ToArray()) + extraPart;
                    break;
            }

            return txnRefCode;
        }

        public static string GenerateAttachValueForTxnRefCode(ItemGenerate item)
        {
            string attachValue = $"[{AttachValueEnum.ClassId}:{item.ClassId}][{AttachValueEnum.StudentId}:{string.Join(", ", item.StudentIdList)}]";

            if (item.CartItemId != default)
            {
                attachValue += $"[{AttachValueEnum.CartItemId}:{item.CartItemId}]";
            }

            return Encrypt(attachValue);
        }

        public static Dictionary<string, List<string>> ExtractAttachValueFromSignature(string signature)
        {
            string attachValue = Decrypt(signature.Substring(36));

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
        private static string Encrypt(string input)
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

            string EncryptionKey = configuration.GetSection("Encryption:Key").Value!;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(EncryptionKey);
                aesAlg.IV = new byte[16];

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(input);
                        }
                    }

                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        public static string Decrypt(string input)
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

            string EncryptionKey = configuration.GetSection("Encryption:Key").Value!;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(EncryptionKey);
                aesAlg.IV = new byte[16];

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(input)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
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

        public static string GenerateJsonString(List<(string, string)> values)
        {
            StringBuilder json = new StringBuilder("{");
            bool first = true;

            foreach (var (name, value) in values)
            {
                if (!first)
                {
                    json.Append(",");
                }
                else
                {
                    first = false;
                }

                if (value.Contains(','))
                {
                    var listValues = value.Split(',').Select(item => $"\"{item.Trim()}\"");
                    json.Append($"\"{name}\": [{string.Join(", ", listValues)}]");
                }
                else
                {
                    json.Append($"\"{name}\": \"{value}\"");
                }
            }

            json.Append("}");

            return json.ToString();
        }
    }
}
