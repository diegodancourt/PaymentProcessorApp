using System.Text.RegularExpressions;

namespace CheckReader.Parsers
{
    public static partial class PayeeParser
    {
        public static string ExtractPayee(string text)
        {
            // Normalize whitespace first - OCR often breaks lines oddly
            var normalizedText = WhitespaceNormalizerRegex().Replace(text, " ");

            // Look for "Pay to the order of" with flexible whitespace
            // Handle OCR merging "orderof" into one word
            var patterns = new[]
            {
                @"Pay\s+to\s+the\s+order\s*of\s*[:\-]?\s*([A-Za-z\s\.]+?)(?:\s*\$|\s+\d+[,\.]|\s*Date|\s*One|\s*Two|\s*Three|\s*Four|\s*Five)",
                @"Pay\s+to\s+the\s+orderof\s*[:\-]?\s*([A-Za-z\s\.]+?)(?:\s*\$|\s+\d+[,\.]|\s*Date|\s*One|\s*Two|\s*Three|\s*Four|\s*Five)", // merged "orderof"
                @"orderof\s+([A-Za-z\s\.]+?)(?:\s*\$|\s+\d+[,\.]|\s*Date|\s*One|\s*Two|\s*Three|\s*Four|\s*Five)", // just "orderof Name"
                @"order\s*of\s*[:\-]?\s*([A-Za-z\s\.]+?)(?:\s*\$|\s+\d+[,\.])",
            };

            var payeeList = from pattern in patterns
                select Regex.Match(normalizedText, pattern, RegexOptions.IgnoreCase)
                into match
                where match.Success && match.Groups[1].Success
                select match.Groups[1].Value.Trim()
                into payee
                select ExtraWhitespaceRegex().Replace(payee, " ")
                into payee
                select payee.Trim();

            return payeeList.FirstOrDefault(payee => payee.Length >= 3 && !DigitsOnlyRegex().IsMatch(payee)) ??
                   string.Empty;
        }

        [GeneratedRegex(@"\s+")]
        private static partial Regex WhitespaceNormalizerRegex();

        [GeneratedRegex(@"\s+")]
        private static partial Regex ExtraWhitespaceRegex();

        [GeneratedRegex(@"^\d+$")]
        private static partial Regex DigitsOnlyRegex();
    }
}