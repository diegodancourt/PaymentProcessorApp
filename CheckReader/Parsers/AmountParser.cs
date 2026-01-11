using System.Text.RegularExpressions;

namespace CheckReader.Parsers
{
    public static class AmountParser
    {
        public static decimal ExtractAmount(string text)
        {
            // Try to find dollar amounts - look for $ followed by numbers
            var patterns = new[]
            {
                @"\$\s*(\d{1,3}(?:,\d{3})*(?:\.\d{2})?)", // $1,234.56
                @"(\d{1,3}(?:,\d{3})*\.\d{2})", // 1,234.56
            };

            var values = from pattern in patterns
                select Regex.Match(text, pattern)
                into match
                where match.Success
                select match.Groups[1].Success ? match.Groups[1].Value : match.Value;
            
            var value = values.FirstOrDefault();
            var checkAmount = 0m;

            if (value == null)
            {
                return checkAmount;
            }

            // Clean and parse
            value = value.Replace("$", "").Replace(",", "").Trim();
            if (decimal.TryParse(value, out var amount))
            {
                checkAmount = amount;
            }

            return checkAmount;
        }
    }
}