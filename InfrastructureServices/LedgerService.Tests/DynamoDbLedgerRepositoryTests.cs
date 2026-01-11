using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LedgerService.Tests;

public class DynamoDbLedgerRepositoryTests
{
    private readonly Mock<IAmazonDynamoDB> _mockDynamoDbClient;
    private readonly Mock<ILogger<DynamoDbLedgerRepository>> _mockLogger;
    private readonly DynamoDbSettings _settings;
    private readonly DynamoDbLedgerRepository _repository;

    public DynamoDbLedgerRepositoryTests()
    {
        _mockDynamoDbClient = new Mock<IAmazonDynamoDB>();
        _mockLogger = new Mock<ILogger<DynamoDbLedgerRepository>>();
        _settings = new DynamoDbSettings
        {
            TableName = "PaymentLedger",
            ServiceUrl = "http://localhost:8000",
            Region = "us-east-1"
        };

        _repository = new DynamoDbLedgerRepository(
            _mockDynamoDbClient.Object,
            _mockLogger.Object,
            Options.Create(_settings)
        );
    }

    [Fact]
    public async Task SavePaymentStatusAsync_ShouldCallPutItemAsync()
    {
        // Arrange
        var paymentStatus = new PaymentStatus
        {
            PaymentId = "PAY-12345",
            CustomerId = Guid.NewGuid(),
            Amount = 150.75m,
            Status = "Success",
            Timestamp = DateTime.UtcNow,
            PaymentMethod = "CreditCard"
        };

        _mockDynamoDbClient
            .Setup(x => x.PutItemAsync(It.IsAny<PutItemRequest>(), default))
            .ReturnsAsync(new PutItemResponse());

        // Act
        await _repository.SavePaymentStatusAsync(paymentStatus);

        // Assert
        _mockDynamoDbClient.Verify(
            x => x.PutItemAsync(
                It.Is<PutItemRequest>(r =>
                    r.TableName == _settings.TableName &&
                    r.Item["PaymentId"].S == paymentStatus.PaymentId &&
                    r.Item["CustomerId"].S == paymentStatus.CustomerId.ToString() &&
                    r.Item["Amount"].N == paymentStatus.Amount.ToString("F2") &&
                    r.Item["Status"].S == paymentStatus.Status &&
                    r.Item["PaymentMethod"].S == paymentStatus.PaymentMethod
                ),
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task SavePaymentStatusAsync_WithoutOptionalFields_ShouldOnlySaveRequiredFields()
    {
        // Arrange
        var paymentStatus = new PaymentStatus
        {
            PaymentId = "PAY-12345",
            CustomerId = Guid.NewGuid(),
            Amount = 150.75m,
            Status = "Success",
            Timestamp = DateTime.UtcNow
        };

        _mockDynamoDbClient
            .Setup(x => x.PutItemAsync(It.IsAny<PutItemRequest>(), default))
            .ReturnsAsync(new PutItemResponse());

        // Act
        await _repository.SavePaymentStatusAsync(paymentStatus);

        // Assert
        _mockDynamoDbClient.Verify(
            x => x.PutItemAsync(
                It.Is<PutItemRequest>(r =>
                    r.TableName == _settings.TableName &&
                    r.Item["PaymentId"].S == paymentStatus.PaymentId &&
                    !r.Item.ContainsKey("PaymentMethod") &&
                    !r.Item.ContainsKey("ErrorMessage")
                ),
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task SavePaymentStatusAsync_WhenDynamoDbThrowsException_ShouldThrowException()
    {
        // Arrange
        var paymentStatus = new PaymentStatus
        {
            PaymentId = "PAY-12345",
            CustomerId = Guid.NewGuid(),
            Amount = 150.75m,
            Status = "Success",
            Timestamp = DateTime.UtcNow
        };

        _mockDynamoDbClient
            .Setup(x => x.PutItemAsync(It.IsAny<PutItemRequest>(), default))
            .ThrowsAsync(new Exception("DynamoDB error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(
            () => _repository.SavePaymentStatusAsync(paymentStatus)
        );
    }

    [Fact]
    public async Task GetPaymentStatusAsync_WhenPaymentExists_ShouldReturnPaymentStatus()
    {
        // Arrange
        var paymentId = "PAY-12345";
        var customerId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;

        var response = new GetItemResponse
        {
            Item = new Dictionary<string, AttributeValue>
            {
                ["PaymentId"] = new AttributeValue { S = paymentId },
                ["CustomerId"] = new AttributeValue { S = customerId.ToString() },
                ["Amount"] = new AttributeValue { N = "150.75" },
                ["Status"] = new AttributeValue { S = "Success" },
                ["Timestamp"] = new AttributeValue { S = timestamp.ToString("O") },
                ["PaymentMethod"] = new AttributeValue { S = "CreditCard" }
            }
        };

        _mockDynamoDbClient
            .Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), default))
            .ReturnsAsync(response);

        // Act
        var result = await _repository.GetPaymentStatusAsync(paymentId);

        // Assert
        result.Should().NotBeNull();
        result!.PaymentId.Should().Be(paymentId);
        result.CustomerId.Should().Be(customerId);
        result.Amount.Should().Be(150.75m);
        result.Status.Should().Be("Success");
        result.PaymentMethod.Should().Be("CreditCard");
    }

    [Fact]
    public async Task GetPaymentStatusAsync_WhenPaymentNotFound_ShouldReturnNull()
    {
        // Arrange
        var paymentId = "PAY-99999";

        var response = new GetItemResponse
        {
            Item = new Dictionary<string, AttributeValue>()
        };

        _mockDynamoDbClient
            .Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), default))
            .ReturnsAsync(response);

        // Act
        var result = await _repository.GetPaymentStatusAsync(paymentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPaymentStatusAsync_WhenDynamoDbThrowsException_ShouldReturnNull()
    {
        // Arrange
        var paymentId = "PAY-12345";

        _mockDynamoDbClient
            .Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), default))
            .ThrowsAsync(new Exception("DynamoDB error"));

        // Act
        var result = await _repository.GetPaymentStatusAsync(paymentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPaymentStatusAsync_ShouldUseCorrectTableName()
    {
        // Arrange
        var paymentId = "PAY-12345";

        var response = new GetItemResponse
        {
            Item = new Dictionary<string, AttributeValue>()
        };

        _mockDynamoDbClient
            .Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), default))
            .ReturnsAsync(response);

        // Act
        await _repository.GetPaymentStatusAsync(paymentId);

        // Assert
        _mockDynamoDbClient.Verify(
            x => x.GetItemAsync(
                It.Is<GetItemRequest>(r =>
                    r.TableName == _settings.TableName &&
                    r.Key["PaymentId"].S == paymentId
                ),
                default
            ),
            Times.Once
        );
    }
}
