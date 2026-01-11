using System.Text.RegularExpressions;

namespace CheckReader.Parsers
{
    public static class DateParser
    {
        public static DateOnly? ExtractDate(string text)
        {
            // Common date formats on checks
            var patterns = new[]
            {
                @"\b(\d{1,2}[-/]\d{1,2}[-/]\d{2,4})\b", // 12/31/2024
                @"\b([A-Za-z]+,?\s+\d{1,2},?\s+\d{4})\b", // January 15, 2024 or January, 10, 2026
            };

            var dateString = (from pattern in patterns
                select Regex.Match(text, pattern)
                into match
                where match.Success
                select match.Groups[1].Value).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(dateString))
            {
                return null;
            }

            return DateOnly.Parse(dateString);
        }
    }
}