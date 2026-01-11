using System.Text.RegularExpressions;

namespace CheckReader.Parsers
{
    /// <summary>
    ///MICR stands for Magnetic Ink Character Recognition.
    /// It's the line of numbers at the bottom of checks printed in special magnetic ink using a specific font (usually E-13B or CMC-7). The MICR line contains:
    ///
    /// Routing number (9 digits) - identifies the bank
    /// Account number (varies, typically 10-12 digits) - identifies the account holder
    ///     Check number (varies) - matches the check number printed in the top right
    ///
    ///     The format typically looks like: ⑆021000021⑆ 1234567890⑈ 0001
    /// The special characters (⑆, ⑈, ⑉, ⑊) are delimiters that separate the fields.
    ///
    /// This class parses the MICR line to extract these components.
    /// </summary>
    public static partial class MicrParser
    {
        public static string ExtractRoutingNumber(string text)
        {
            // Routing numbers are exactly 9 digits
            // OCR often merges digits or adds extra characters
            // Look for 9 consecutive digits in a longer string of digits

            var patterns = new[]
            {
                @"\|[:\s]*(\d{9})\|", // |:123456789|: format
                @"(?:^|\D)(\d{9})(?:\D|$)", // 9 digits with non-digit boundaries
                @"\b(\d{9})\b", // Word boundary format
            };

            foreach (var pattern in patterns)
            {
                var matches = Regex.Matches(text, pattern, RegexOptions.Multiline);
                foreach (Match match in matches)
                {
                    string candidate = match.Groups[1].Value;
                    // Routing numbers have a checksum - you could validate here
                    // For now, just return the first 9-digit sequence
                    if (candidate.Length == 9)
                        return candidate;
                }
            }

            // Fallback: Look for first sequence of exactly 9 digits in any digit string
            var digitSequence = Regex.Match(text, @"\d+");
            while (digitSequence.Success)
            {
                if (digitSequence.Value.Length >= 9)
                {
                    // Extract first 9 digits from the sequence
                    return digitSequence.Value.Substring(0, 9);
                }

                digitSequence = digitSequence.NextMatch();
            }

            return null;
        }

        public static string ExtractAccountNumber(string text)
        {
            // Account numbers typically follow routing number
            // Usually 8-17 digits, but OCR might merge them together

            // First try to find routing + account pattern
            var match = Regex.Match(text, @"(?:^|\D)(\d{9})\D*(\d{8,17})(?:\D|$)", RegexOptions.Multiline);
            if (match.Success && match.Groups[2].Success)
            {
                return match.Groups[2].Value;
            }

            // Look for the MICR line pattern more flexibly
            // Format is usually: |:routing|: account check|
            match = Regex.Match(text, @"\d{9}\s+(\d{8,17})\s+\d{3,5}");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            // Fallback: Find second sequence of 8+ digits after finding a 9-digit sequence
            var digitMatches = OnlyDigitsRegex().Matches(text);
            if (digitMatches.Count >= 2)
            {
                // Skip the first (likely routing number) and take the second
                var candidate = digitMatches[1].Value;
                return candidate.Length <= 17 ? candidate : candidate[..17]; // Truncate if too long
            }

            return null;
        }

        public static string ExtractCheckNumber(string text)
        {
            // Check number patterns - be more flexible with whitespace
            var patterns = new[]
            {
                @"(?:Check\s*(?:#|No\.?|Number)\s*)(\d{3,5})", // "Check #1234"
                @"\d{8,17}\s+(\d{3,5})\s*\|", // MICR line: account followed by check number and pipe
                @"\d{8,17}\s+(\d{3,5})(?:\s|$)", // MICR line: account followed by check number
                @"(?:^|\n)\s*(\d{3,5})\s*(?:\n|$)", // Isolated number at line start/end
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (match.Success)
                    return match.Groups[1].Value;
            }

            return null;
        }

        [GeneratedRegex(@"\d{8,}")]
        private static partial Regex OnlyDigitsRegex();
    }
}