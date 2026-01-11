using System.Text.Json;
using CheckService.Domain;
using CheckService.Infrastructure.Producers;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CheckService.Tests.Infrastructure.Producers;

public class PaymentStatusProducerTests
{
    private readonly Mock<ILogger<PaymentStatusProducer>> _loggerMock;
    private readonly Mock<IProducer<Null, string>> _producerMock;
    private readonly IOptions<KafkaSettings> _kafkaSettings;
    private readonly PaymentStatusProducer _sut;

    public PaymentStatusProducerTests()
    {
        _loggerMock = new Mock<ILogger<PaymentStatusProducer>>();
        _producerMock = new Mock<IProducer<Null, string>>();
        _kafkaSettings = Options.Create(new KafkaSettings
        {
            BootstrapServers = "localhost:9092",
            GroupId = "test-group",
            ConsumerTopic = "test-consumer-topic",
            ProducerTopic = "test-producer-topic",
            AutoOffsetReset = true,
            EnableAutoCommit = false
        });

        _sut = new PaymentStatusProducer(_loggerMock.Object, _producerMock.Object, _kafkaSettings);
    }

    [Fact]
    public async Task PublishPaymentStatusAsync_ShouldSerializeAndPublishMessage()
    {
        // Arrange
        var paymentStatus = new PaymentStatus
        {
            PaymentId = "payment-123",
            CustomerId = Guid.NewGuid(),
            Amount = 150.75m,
            Status = "Success",
            Timestamp = DateTime.UtcNow,
            PaymentMethod = "Check"
        };

        var deliveryResult = new DeliveryResult<Null, string>
        {
            TopicPartitionOffset = new TopicPartitionOffset("test-producer-topic", new Partition(0), new Offset(10))
        };

        _producerMock
            .Setup(p => p.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<Message<Null, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deliveryResult);

        // Act
        await _sut.PublishPaymentStatusAsync(paymentStatus, CancellationToken.None);

        // Assert
        _producerMock.Verify(p => p.ProduceAsync(
            "test-producer-topic",
            It.Is<Message<Null, string>>(m => ValidateMessage(m, paymentStatus)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishPaymentStatusAsync_ShouldThrowException_WhenProducerFails()
    {
        // Arrange
        var paymentStatus = new PaymentStatus
        {
            PaymentId = "payment-456",
            CustomerId = Guid.NewGuid(),
            Amount = 200.00m,
            Status = "Success",
            Timestamp = DateTime.UtcNow,
            PaymentMethod = "Check"
        };

        _producerMock
            .Setup(p => p.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<Message<Null, string>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ProduceException<Null, string>(
                new Error(ErrorCode.Local_QueueFull),
                new DeliveryResult<Null, string>()));

        // Act & Assert
        await Assert.ThrowsAsync<ProduceException<Null, string>>(
            () => _sut.PublishPaymentStatusAsync(paymentStatus, CancellationToken.None));
    }

    [Fact]
    public async Task PublishFailureStatusAsync_ShouldCreateAndPublishFailedStatus()
    {
        // Arrange
        var request = new CheckPaymentRequest
        {
            PaymentId = "payment-789",
            CustomerId = Guid.NewGuid(),
            ImageData = new byte[] { 1, 2, 3 }
        };

        var errorMessage = "OCR processing failed";

        var deliveryResult = new DeliveryResult<Null, string>
        {
            TopicPartitionOffset = new TopicPartitionOffset("test-producer-topic", new Partition(0), new Offset(11))
        };

        _producerMock
            .Setup(p => p.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<Message<Null, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deliveryResult);

        // Act
        await _sut.PublishFailureStatusAsync(request, errorMessage, CancellationToken.None);

        // Assert
        _producerMock.Verify(p => p.ProduceAsync(
            "test-producer-topic",
            It.Is<Message<Null, string>>(m => ValidateFailureMessage(m, request, errorMessage)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishFailureStatusAsync_ShouldNotThrow_WhenProducerFails()
    {
        // Arrange
        var request = new CheckPaymentRequest
        {
            PaymentId = "payment-999",
            CustomerId = Guid.NewGuid(),
            ImageData = new byte[] { 1, 2, 3 }
        };

        _producerMock
            .Setup(p => p.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<Message<Null, string>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ProduceException<Null, string>(
                new Error(ErrorCode.Local_QueueFull),
                new DeliveryResult<Null, string>()));

        // Act & Assert - should not throw
        await _sut.PublishFailureStatusAsync(request, "Test error", CancellationToken.None);

        // Verify error was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(2)); // Once for PublishPaymentStatusAsync, once for PublishFailureStatusAsync
    }

    private bool ValidateMessage(Message<Null, string> message, PaymentStatus expectedStatus)
    {
        var deserializedStatus = JsonSerializer.Deserialize<PaymentStatus>(message.Value);
        return deserializedStatus != null &&
               deserializedStatus.PaymentId == expectedStatus.PaymentId &&
               deserializedStatus.CustomerId == expectedStatus.CustomerId &&
               deserializedStatus.Amount == expectedStatus.Amount &&
               deserializedStatus.Status == expectedStatus.Status &&
               deserializedStatus.PaymentMethod == expectedStatus.PaymentMethod;
    }

    private bool ValidateFailureMessage(Message<Null, string> message, CheckPaymentRequest request, string errorMessage)
    {
        var deserializedStatus = JsonSerializer.Deserialize<PaymentStatus>(message.Value);
        return deserializedStatus != null &&
               deserializedStatus.PaymentId == request.PaymentId &&
               deserializedStatus.CustomerId == request.CustomerId &&
               deserializedStatus.Amount == 0 &&
               deserializedStatus.Status == "Failed" &&
               deserializedStatus.ErrorMessage == errorMessage &&
               deserializedStatus.PaymentMethod == "Check";
    }
}