using CheckReader.Parsers;
using Xunit;

namespace CheckReader.Tests.Parsers;

public class DateParserTests
{
    [Fact]
    public void ExtractDate_WithSlashFormat_ReturnsCorrectDate()
    {
        // Arrange
        const string text = "Date: 12/31/2024";

        // Act
        var result = DateParser.ExtractDate(text);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new DateOnly(2024, 12, 31), result);
    }

    [Fact]
    public void ExtractDate_WithDashFormat_ReturnsCorrectDate()
    {
        // Arrange
        const string text = "Date: 01-15-2024";

        // Act
        var result = DateParser.ExtractDate(text);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new DateOnly(2024, 1, 15), result);
    }

    [Fact]
    public void ExtractDate_WithTwoDigitYear_ReturnsCorrectDate()
    {
        // Arrange
        const string text = "Date: 6/1/24";

        // Act
        var result = DateParser.ExtractDate(text);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new DateOnly(2024, 6, 1), result);
    }

    [Fact]
    public void ExtractDate_WithWrittenMonth_ReturnsCorrectDate()
    {
        // Arrange
        const string text = "January 15, 2024";

        // Act
        var result = DateParser.ExtractDate(text);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new DateOnly(2024, 1, 15), result);
    }

    [Fact]
    public void ExtractDate_WithWrittenMonthAndComma_ReturnsCorrectDate()
    {
        // Arrange
        const string text = "Date: March, 10, 2025";

        // Act
        var result = DateParser.ExtractDate(text);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new DateOnly(2025, 3, 10), result);
    }

    [Fact]
    public void ExtractDate_WithNoDate_ReturnsNull()
    {
        // Arrange
        const string text = "Pay to the order of John Doe";

        // Act
        var result = DateParser.ExtractDate(text);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ExtractDate_WithEmptyString_ReturnsNull()
    {
        // Arrange
        const string text = "";

        // Act
        var result = DateParser.ExtractDate(text);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ExtractDate_WithDateInLongerText_ReturnsCorrectDate()
    {
        // Arrange
        const string text = "Check #1234\nDate: 05/20/2024\nPay to the order of Jane Smith";

        // Act
        var result = DateParser.ExtractDate(text);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new DateOnly(2024, 5, 20), result);
    }
}