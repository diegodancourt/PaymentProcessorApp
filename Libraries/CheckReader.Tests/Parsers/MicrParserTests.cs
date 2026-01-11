using CheckReader.Parsers;
using Xunit;

namespace CheckReader.Tests.Parsers;

public class MicrParserTests
{
    public class ExtractRoutingNumberTests
    {
        [Fact]
        public void ExtractRoutingNumber_WithPipeFormat_ReturnsCorrectNumber()
        {
            // Arrange
            const string text = "|:021000021|: 1234567890 0001";

            // Act
            var result = MicrParser.ExtractRoutingNumber(text);

            // Assert
            Assert.Equal("021000021", result);
        }

        [Fact]
        public void ExtractRoutingNumber_WithNineDigitSequence_ReturnsCorrectNumber()
        {
            // Arrange
            const string text = "Routing: 123456789";

            // Act
            var result = MicrParser.ExtractRoutingNumber(text);

            // Assert
            Assert.Equal("123456789", result);
        }

        [Fact]
        public void ExtractRoutingNumber_WithLongerSequence_ReturnsFirstNineDigits()
        {
            // Arrange
            const string text = "MICR: 12345678901234567890";

            // Act
            var result = MicrParser.ExtractRoutingNumber(text);

            // Assert
            Assert.Equal("123456789", result);
        }

        [Fact]
        public void ExtractRoutingNumber_WithNoDigits_ReturnsNull()
        {
            // Arrange
            const string text = "No routing number here";

            // Act
            var result = MicrParser.ExtractRoutingNumber(text);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ExtractRoutingNumber_WithEmptyString_ReturnsNull()
        {
            // Arrange
            const string text = "";

            // Act
            var result = MicrParser.ExtractRoutingNumber(text);

            // Assert
            Assert.Null(result);
        }
    }

    public class ExtractAccountNumberTests
    {
        [Fact]
        public void ExtractAccountNumber_WithStandardMicrLine_ReturnsCorrectNumber()
        {
            // Arrange
            const string text = "021000021 1234567890 0001";

            // Act
            var result = MicrParser.ExtractAccountNumber(text);

            // Assert
            Assert.Equal("1234567890", result);
        }

        [Fact]
        public void ExtractAccountNumber_WithRoutingAndAccount_ReturnsCorrectNumber()
        {
            // Arrange
            const string text = "123456789 98765432101234";

            // Act
            var result = MicrParser.ExtractAccountNumber(text);

            // Assert
            Assert.Equal("98765432101234", result);
        }

        [Fact]
        public void ExtractAccountNumber_WithNoAccountNumber_ReturnsNull()
        {
            // Arrange
            const string text = "123456789";

            // Act
            var result = MicrParser.ExtractAccountNumber(text);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ExtractAccountNumber_WithEmptyString_ReturnsNull()
        {
            // Arrange
            const string text = "";

            // Act
            var result = MicrParser.ExtractAccountNumber(text);

            // Assert
            Assert.Null(result);
        }
    }

    public class ExtractCheckNumberTests
    {
        [Fact]
        public void ExtractCheckNumber_WithCheckHashFormat_ReturnsCorrectNumber()
        {
            // Arrange
            const string text = "Check #1234";

            // Act
            var result = MicrParser.ExtractCheckNumber(text);

            // Assert
            Assert.Equal("1234", result);
        }

        [Fact]
        public void ExtractCheckNumber_WithCheckNoFormat_ReturnsCorrectNumber()
        {
            // Arrange
            const string text = "Check No. 5678";

            // Act
            var result = MicrParser.ExtractCheckNumber(text);

            // Assert
            Assert.Equal("5678", result);
        }

        [Fact]
        public void ExtractCheckNumber_WithCheckNumberFormat_ReturnsCorrectNumber()
        {
            // Arrange
            const string text = "Check Number 9012";

            // Act
            var result = MicrParser.ExtractCheckNumber(text);

            // Assert
            Assert.Equal("9012", result);
        }

        [Fact]
        public void ExtractCheckNumber_FromMicrLine_ReturnsCorrectNumber()
        {
            // Arrange
            const string text = "021000021 1234567890 0001|";

            // Act
            var result = MicrParser.ExtractCheckNumber(text);

            // Assert
            Assert.Equal("0001", result);
        }

        [Fact]
        public void ExtractCheckNumber_WithNoCheckNumber_ReturnsNull()
        {
            // Arrange
            const string text = "Pay to the order of John Doe";

            // Act
            var result = MicrParser.ExtractCheckNumber(text);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ExtractCheckNumber_WithEmptyString_ReturnsNull()
        {
            // Arrange
            const string text = "";

            // Act
            var result = MicrParser.ExtractCheckNumber(text);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ExtractCheckNumber_WithIsolatedNumber_ReturnsCorrectNumber()
        {
            // Arrange
            const string text = "\n1234\n";

            // Act
            var result = MicrParser.ExtractCheckNumber(text);

            // Assert
            Assert.Equal("1234", result);
        }
    }
}