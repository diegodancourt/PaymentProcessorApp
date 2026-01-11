using CheckReader.Parsers;
using Xunit;

namespace CheckReader.Tests.Parsers;

public class PayeeParserTests
{
    [Fact]
    public void ExtractPayee_WithStandardFormat_ReturnsCorrectPayee()
    {
        // Arrange
        const string text = "Pay to the order of John Doe $500.00";

        // Act
        var result = PayeeParser.ExtractPayee(text);

        // Assert
        Assert.Equal("John Doe", result);
    }

    [Fact]
    public void ExtractPayee_WithColonSeparator_ReturnsCorrectPayee()
    {
        // Arrange
        const string text = "Pay to the order of: Jane Smith $1,000.00";

        // Act
        var result = PayeeParser.ExtractPayee(text);

        // Assert
        Assert.Equal("Jane Smith", result);
    }

    [Fact]
    public void ExtractPayee_WithMergedOrderOf_ReturnsCorrectPayee()
    {
        // Arrange
        const string text = "Pay to the orderof Bob Johnson $250.00";

        // Act
        var result = PayeeParser.ExtractPayee(text);

        // Assert
        Assert.Equal("Bob Johnson", result);
    }

    [Fact]
    public void ExtractPayee_WithCompanyName_ReturnsCorrectPayee()
    {
        // Arrange
        const string text = "Pay to the order of Acme Corp. $5,000.00";

        // Act
        var result = PayeeParser.ExtractPayee(text);

        // Assert
        Assert.Equal("Acme Corp.", result);
    }

    [Fact]
    public void ExtractPayee_WithExtraWhitespace_ReturnsNormalizedPayee()
    {
        // Arrange
        const string text = "Pay to the order of   John    Doe   $100.00";

        // Act
        var result = PayeeParser.ExtractPayee(text);

        // Assert
        Assert.Equal("John Doe", result);
    }

    [Fact]
    public void ExtractPayee_WithDateTerminator_ReturnsCorrectPayee()
    {
        // Arrange
        const string text = "Pay to the order of Mary Johnson Date: 01/01/2024";

        // Act
        var result = PayeeParser.ExtractPayee(text);

        // Assert
        Assert.Equal("Mary Johnson", result);
    }

    [Fact]
    public void ExtractPayee_WithWrittenAmountTerminator_ReturnsCorrectPayee()
    {
        // Arrange
        const string text = "Pay to the order of Sarah Lee One Thousand Dollars";

        // Act
        var result = PayeeParser.ExtractPayee(text);

        // Assert
        Assert.Equal("Sarah Lee", result);
    }

    [Fact]
    public void ExtractPayee_WithNoPayee_ReturnsEmptyString()
    {
        // Arrange
        const string text = "Check #1234 Amount: $500.00";

        // Act
        var result = PayeeParser.ExtractPayee(text);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ExtractPayee_WithEmptyString_ReturnsEmptyString()
    {
        // Arrange
        const string text = "";

        // Act
        var result = PayeeParser.ExtractPayee(text);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ExtractPayee_WithJustOrderOf_ReturnsCorrectPayee()
    {
        // Arrange
        const string text = "orderof Michael Brown $750.00";

        // Act
        var result = PayeeParser.ExtractPayee(text);

        // Assert
        Assert.Equal("Michael Brown", result);
    }

    [Fact]
    public void ExtractPayee_WithMultiLineText_ReturnsCorrectPayee()
    {
        // Arrange
        const string text = "Check #5678\nPay to the order of Alice Wong\n$2,500.00";

        // Act
        var result = PayeeParser.ExtractPayee(text);

        // Assert
        Assert.Equal("Alice Wong", result);
    }
}