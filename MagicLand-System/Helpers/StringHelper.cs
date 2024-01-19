using MagicLand_System.Enums;
using System.Text.RegularExpressions;

namespace MagicLand_System.Helpers
{
    public class StringHelper
    {
        public static List<string> ExtractValuesFromTransactionDescription(string input, string key, bool noSpace)
        {
            string pattern = $@"\[{key}:\s*([^]]+)\s*]";
            Match match = Regex.Match(input, pattern);

            if (match.Success)
            {
                string values = match.Groups[1].Value.Trim();
                if (noSpace)
                {
                    return values.Split(',').Select(s => TrimStringAndNoSpace(s)).ToList();
                }
                return values.Split(',').Select(s => s.Trim()).ToList();
            }

            return new List<string>();
        }

        public static string TrimStringAndNoSpace(string input)
        {
            return input.Trim().Replace(" ", "");
        }

        public static string GenerateRadomTransactionCode(TransactionTypeEnum type)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();

            string randomCode = "";

            switch (type)
            {
                case TransactionTypeEnum.Refund:
                    randomCode += "RF ";
                    break;

                case TransactionTypeEnum.Payment:
                    randomCode += "PM ";
                    break;

                case TransactionTypeEnum.TopUp:
                    randomCode += "TU ";
                    break;
            }
            randomCode += new string(Enumerable.Repeat(chars, 11).Select(s => s[random.Next(s.Length)]).ToArray());

            return randomCode;
        }

        public static string UpdatePartValueOfTransactionDescription(string input, string value, string key)
        {
            string pattern = $@"\[{key}:\s*([^]]+)\s*]";
            Match match = Regex.Match(input, pattern);
            if (match.Success)
            {
                int startIndex = input.IndexOf($"[{key}:");

                if (startIndex != -1)
                {
                    int endIndex = input.IndexOf("]", startIndex);

                    if (endIndex != -1)
                    {
                        string oldValue = input.Substring(startIndex, endIndex - startIndex + 1);

                        string newValue = $"[{key}: {value}]";

                        string updatedString = input.Replace(oldValue, newValue);

                        return updatedString;
                    }
                }
            }
            return "";
        }
    }
}
