using CheckReader.Services;
using Moq;
using Xunit;

namespace CheckReader.Tests.Services;

public class CheckReaderTests
{
    [Fact]
    public async Task ReadCheckAsync_WithValidCheckText_ReturnsCheckWithAllFields()
    {
        // Arrange
        const string ocrText = """
            Check #1234
            Date: 01/15/2024
            Pay to the order of John Smith $1,500.00
            One Thousand Five Hundred Dollars
            021000021 9876543210 1234
            """;

        var mockOcrEngine = new Mock<IOcrEngine>();
        mockOcrEngine
            .Setup(x => x.ExtractTextAsync(It.IsAny<byte[]>()))
            .ReturnsAsync(ocrText);

        var checkReader = new CheckReader.Services.CheckReader(mockOcrEngine.Object);

        // Act
        var result = await checkReader.ReadCheckAsync([0x00]);

        // Assert
        Assert.Equal("John Smith", result.Payee.Name);
        Assert.Equal(1500.00m, result.Amount.Value);
        Assert.Equal(new DateOnly(2024, 1, 15), result.Date);
        Assert.Equal("021000021", result.Micr.RoutingNumber);
        Assert.Equal("9876543210", result.Micr.AccountNumber);
        Assert.Equal("1234", result.Micr.CheckNumber);
        Assert.Equal(ocrText, result.RawText);
    }

    [Fact]
    public async Task ReadCheckAsync_WithAmountOnly_ReturnsCheckWithAmount()
    {
        // Arrange
        const string ocrText = "Amount: $250.00";

        var mockOcrEngine = new Mock<IOcrEngine>();
        mockOcrEngine
            .Setup(x => x.ExtractTextAsync(It.IsAny<byte[]>()))
            .ReturnsAsync(ocrText);

        var checkReader = new CheckReader.Services.CheckReader(mockOcrEngine.Object);

        // Act
        var result = await checkReader.ReadCheckAsync([0x00]);

        // Assert
        Assert.Equal(250.00m, result.Amount.Value);
        Assert.Equal("USD", result.Amount.Currency);
    }

    [Fact]
    public async Task ReadCheckAsync_WithMicrLineOnly_ReturnsCheckWithMicrData()
    {
        // Arrange
        const string ocrText = "123456789 1234567890 0001";

        var mockOcrEngine = new Mock<IOcrEngine>();
        mockOcrEngine
            .Setup(x => x.ExtractTextAsync(It.IsAny<byte[]>()))
            .ReturnsAsync(ocrText);

        var checkReader = new CheckReader.Services.CheckReader(mockOcrEngine.Object);

        // Act
        var result = await checkReader.ReadCheckAsync([0x00]);

        // Assert
        Assert.Equal("123456789", result.Micr.RoutingNumber);
        Assert.Equal("1234567890", result.Micr.AccountNumber);
        Assert.Equal("0001", result.Micr.CheckNumber);
    }

    [Fact]
    public async Task ReadCheckAsync_WithPayeeOnly_ReturnsCheckWithPayee()
    {
        // Arrange
        const string ocrText = "Pay to the order of Jane Doe $100.00";

        var mockOcrEngine = new Mock<IOcrEngine>();
        mockOcrEngine
            .Setup(x => x.ExtractTextAsync(It.IsAny<byte[]>()))
            .ReturnsAsync(ocrText);

        var checkReader = new CheckReader.Services.CheckReader(mockOcrEngine.Object);

        // Act
        var result = await checkReader.ReadCheckAsync([0x00]);

        // Assert
        Assert.Equal("Jane Doe", result.Payee.Name);
    }

    [Fact]
    public async Task ReadCheckAsync_WithDateOnly_ReturnsCheckWithDate()
    {
        // Arrange
        const string ocrText = "Date: March 15, 2024";

        var mockOcrEngine = new Mock<IOcrEngine>();
        mockOcrEngine
            .Setup(x => x.ExtractTextAsync(It.IsAny<byte[]>()))
            .ReturnsAsync(ocrText);

        var checkReader = new CheckReader.Services.CheckReader(mockOcrEngine.Object);

        // Act
        var result = await checkReader.ReadCheckAsync([0x00]);

        // Assert
        Assert.Equal(new DateOnly(2024, 3, 15), result.Date);
    }

    [Fact]
    public async Task ReadCheckAsync_WithEmptyText_ReturnsCheckWithDefaults()
    {
        // Arrange
        const string ocrText = "";

        var mockOcrEngine = new Mock<IOcrEngine>();
        mockOcrEngine
            .Setup(x => x.ExtractTextAsync(It.IsAny<byte[]>()))
            .ReturnsAsync(ocrText);

        var checkReader = new CheckReader.Services.CheckReader(mockOcrEngine.Object);

        // Act
        var result = await checkReader.ReadCheckAsync([0x00]);

        // Assert
        Assert.Equal(0m, result.Amount.Value);
        Assert.Equal(string.Empty, result.Payee.Name);
        Assert.Null(result.Date);
        Assert.Equal(string.Empty, result.RawText);
    }

    [Fact]
    public async Task ReadCheckAsync_CallsOcrEngineWithImageData()
    {
        // Arrange
        byte[] imageData = [0x89, 0x50, 0x4E, 0x47];

        var mockOcrEngine = new Mock<IOcrEngine>();
        mockOcrEngine
            .Setup(x => x.ExtractTextAsync(It.IsAny<byte[]>()))
            .ReturnsAsync("test");

        var checkReader = new CheckReader.Services.CheckReader(mockOcrEngine.Object);

        // Act
        await checkReader.ReadCheckAsync(imageData);

        // Assert
        mockOcrEngine.Verify(x => x.ExtractTextAsync(imageData), Times.Once);
    }

    [Fact]
    public async Task ReadCheckAsync_WithLargeAmount_ReturnsCorrectAmount()
    {
        // Arrange
        const string ocrText = "Pay to the order of Acme Corp $123,456.78";

        var mockOcrEngine = new Mock<IOcrEngine>();
        mockOcrEngine
            .Setup(x => x.ExtractTextAsync(It.IsAny<byte[]>()))
            .ReturnsAsync(ocrText);

        var checkReader = new CheckReader.Services.CheckReader(mockOcrEngine.Object);

        // Act
        var result = await checkReader.ReadCheckAsync([0x00]);

        // Assert
        Assert.Equal(123456.78m, result.Amount.Value);
        Assert.Equal("Acme Corp", result.Payee.Name);
    }
}