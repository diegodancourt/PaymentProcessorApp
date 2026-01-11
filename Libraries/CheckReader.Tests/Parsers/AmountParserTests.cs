using CheckReader.Parsers;
using Xunit;

namespace CheckReader.Tests.Parsers;

public class AmountParserTests
{
    [Fact]
    public void ExtractAmount_WithDollarSignAndCents_ReturnsCorrectAmount()
    {
        // Arrange
        const string text = "Pay to the order of John Doe $1,234.56";

        // Act
        var result = AmountParser.ExtractAmount(text);

        // Assert
        Assert.Equal(1234.56m, result);
    }

    [Fact]
    public void ExtractAmount_WithDollarSignNoComma_ReturnsCorrectAmount()
    {
        // Arrange
        const string text = "Amount: $500.00";

        // Act
        var result = AmountParser.ExtractAmount(text);

        // Assert
        Assert.Equal(500.00m, result);
    }

    [Fact]
    public void ExtractAmount_WithoutDollarSign_ReturnsCorrectAmount()
    {
        // Arrange
        const string text = "Total amount 1,234.56 dollars";

        // Act
        var result = AmountParser.ExtractAmount(text);

        // Assert
        Assert.Equal(1234.56m, result);
    }

    [Fact]
    public void ExtractAmount_WithSpaceAfterDollarSign_ReturnsCorrectAmount()
    {
        // Arrange
        const string text = "Check for $ 99.99";

        // Act
        var result = AmountParser.ExtractAmount(text);

        // Assert
        Assert.Equal(99.99m, result);
    }

    [Fact]
    public void ExtractAmount_WithLargeAmount_ReturnsCorrectAmount()
    {
        // Arrange
        const string text = "Payment of $123,456.78 received";

        // Act
        var result = AmountParser.ExtractAmount(text);

        // Assert
        Assert.Equal(123456.78m, result);
    }

    [Fact]
    public void ExtractAmount_WithNoAmount_ReturnsZero()
    {
        // Arrange
        const string text = "Pay to the order of John Doe";

        // Act
        var result = AmountParser.ExtractAmount(text);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void ExtractAmount_WithEmptyString_ReturnsZero()
    {
        // Arrange
        const string text = "";

        // Act
        var result = AmountParser.ExtractAmount(text);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void ExtractAmount_WithMultipleAmounts_ReturnsFirstAmount()
    {
        // Arrange
        const string text = "Amount $100.00 and $200.00";

        // Act
        var result = AmountParser.ExtractAmount(text);

        // Assert
        Assert.Equal(100.00m, result);
    }
}