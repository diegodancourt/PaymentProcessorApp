using System.Text.Json;
using CheckReader.Domain;
using CheckReader.Services;
using CheckService.Domain;
using CheckService.Infrastructure.Consumers;
using CheckService.Infrastructure.Producers;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CheckService.Tests.Infrastructure.Consumers;

public class PaymentRequestConsumerTests
{
    private readonly Mock<ILogger<PaymentRequestConsumer>> _loggerMock;
    private readonly Mock<IConsumer<Ignore, string>> _consumerMock;
    private readonly Mock<ILogger<PaymentStatusProducer>> _producerLoggerMock;
    private readonly Mock<IProducer<Null, string>> _kafkaProducerMock;
    private readonly PaymentStatusProducer _producer;
    private readonly Mock<ICheckReader> _checkReaderMock;
    private readonly PaymentRequestConsumer _sut;

    public PaymentRequestConsumerTests()
    {
        _loggerMock = new Mock<ILogger<PaymentRequestConsumer>>();
        _consumerMock = new Mock<IConsumer<Ignore, string>>();

        _producerLoggerMock = new Mock<ILogger<PaymentStatusProducer>>();
        _kafkaProducerMock = new Mock<IProducer<Null, string>>();

        var kafkaSettings = Options.Create(new KafkaSettings
        {
            BootstrapServers = "localhost:9092",
            GroupId = "test-group",
            ConsumerTopic = "test-consumer-topic",
            ProducerTopic = "test-producer-topic",
            AutoOffsetReset = true,
            EnableAutoCommit = false
        });

        _producer = new PaymentStatusProducer(
            _producerLoggerMock.Object,
            _kafkaProducerMock.Object,
            kafkaSettings);

        _checkReaderMock = new Mock<ICheckReader>();

        _sut = new PaymentRequestConsumer(
            _loggerMock.Object,
            _consumerMock.Object,
            _producer,
            _checkReaderMock.Object);
    }

    [Fact]
    public async Task ConsumeMessageAsync_ShouldProcessValidMessage_AndPublishSuccessStatus()
    {
        // Arrange
        var paymentId = "payment-123";
        var customerId = Guid.NewGuid();
        var imageData = new byte[] { 1, 2, 3, 4, 5 };

        var request = new CheckPaymentRequest
        {
            PaymentId = paymentId,
            CustomerId = customerId,
            ImageData = imageData
        };

        var checkData = new Check
        {
            Amount = new Amount(100.50m),
            Micr = new Micr("123456789", "987654321", "001"),
            Payee = new Payee("John Doe"),
            Date = DateOnly.FromDateTime(DateTime.Now)
        };

        var messageJson = JsonSerializer.Serialize(request);
        var consumeResult = CreateConsumeResult(messageJson);

        _consumerMock
            .Setup(c => c.Consume(It.IsAny<CancellationToken>()))
            .Returns(consumeResult);

        _checkReaderMock
            .Setup(r => r.ReadCheckAsync(imageData))
            .ReturnsAsync(checkData);

        var deliveryResult = new DeliveryResult<Null, string>
        {
            TopicPartitionOffset = new TopicPartitionOffset("test-producer-topic", new Partition(0), new Offset(10))
        };

        _kafkaProducerMock
            .Setup(p => p.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<Message<Null, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deliveryResult);

        // Act
        await _sut.ConsumeMessageAsync(CancellationToken.None);

        // Assert
        _checkReaderMock.Verify(r => r.ReadCheckAsync(imageData), Times.Once);

        _kafkaProducerMock.Verify(p => p.ProduceAsync(
            "test-producer-topic",
            It.Is<Message<Null, string>>(m => ValidateSuccessMessage(m, paymentId, customerId, 100.50m)),
            It.IsAny<CancellationToken>()), Times.Once);

        _consumerMock.Verify(c => c.Commit(consumeResult), Times.Once);
    }

    [Fact]
    public async Task ConsumeMessageAsync_ShouldReturnEarly_WhenMessageIsNull()
    {
        // Arrange
#pragma warning disable CS8625
        _consumerMock
            .Setup(c => c.Consume(It.IsAny<CancellationToken>()))
            .Returns((ConsumeResult<Ignore, string>?)null);
#pragma warning restore CS8625

        // Act
        await _sut.ConsumeMessageAsync(CancellationToken.None);

        // Assert
        _checkReaderMock.Verify(r => r.ReadCheckAsync(It.IsAny<byte[]>()), Times.Never);
        _kafkaProducerMock.Verify(p => p.ProduceAsync(
            It.IsAny<string>(),
            It.IsAny<Message<Null, string>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConsumeMessageAsync_ShouldReturnEarly_WhenMessageValueIsNull()
    {
        // Arrange
        var consumeResult = new ConsumeResult<Ignore, string>
        {
            Message = new Message<Ignore, string> { Value = null! },
            TopicPartitionOffset = new TopicPartitionOffset("test-topic", new Partition(0), new Offset(0))
        };

        _consumerMock
            .Setup(c => c.Consume(It.IsAny<CancellationToken>()))
            .Returns(consumeResult);

        // Act
        await _sut.ConsumeMessageAsync(CancellationToken.None);

        // Assert
        _checkReaderMock.Verify(r => r.ReadCheckAsync(It.IsAny<byte[]>()), Times.Never);
        _kafkaProducerMock.Verify(p => p.ProduceAsync(
            It.IsAny<string>(),
            It.IsAny<Message<Null, string>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConsumeMessageAsync_ShouldLogWarning_WhenDeserializationReturnsNull()
    {
        // Arrange
        var consumeResult = CreateConsumeResult("null");

        _consumerMock
            .Setup(c => c.Consume(It.IsAny<CancellationToken>()))
            .Returns(consumeResult);

        // Act
        await _sut.ConsumeMessageAsync(CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to deserialize")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _checkReaderMock.Verify(r => r.ReadCheckAsync(It.IsAny<byte[]>()), Times.Never);
    }

    [Fact]
    public async Task ConsumeMessageAsync_ShouldPublishFailureStatus_WhenDeserializationFails()
    {
        // Arrange
        var invalidJson = "{ invalid json }";
        var consumeResult = CreateConsumeResult(invalidJson);

        _consumerMock
            .Setup(c => c.Consume(It.IsAny<CancellationToken>()))
            .Returns(consumeResult);

        // Act
        await _sut.ConsumeMessageAsync(CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error deserializing")),
                It.IsAny<JsonException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _checkReaderMock.Verify(r => r.ReadCheckAsync(It.IsAny<byte[]>()), Times.Never);
    }

    [Fact]
    public async Task ConsumeMessageAsync_ShouldPublishFailureStatus_WhenCheckReaderThrowsException()
    {
        // Arrange
        var paymentId = "payment-456";
        var customerId = Guid.NewGuid();
        var imageData = new byte[] { 1, 2, 3 };

        var request = new CheckPaymentRequest
        {
            PaymentId = paymentId,
            CustomerId = customerId,
            ImageData = imageData
        };

        var messageJson = JsonSerializer.Serialize(request);
        var consumeResult = CreateConsumeResult(messageJson);

        _consumerMock
            .Setup(c => c.Consume(It.IsAny<CancellationToken>()))
            .Returns(consumeResult);

        var exception = new Exception("OCR engine failure");
        _checkReaderMock
            .Setup(r => r.ReadCheckAsync(imageData))
            .ThrowsAsync(exception);

        var deliveryResult = new DeliveryResult<Null, string>
        {
            TopicPartitionOffset = new TopicPartitionOffset("test-producer-topic", new Partition(0), new Offset(10))
        };

        _kafkaProducerMock
            .Setup(p => p.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<Message<Null, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deliveryResult);

        // Act
        await _sut.ConsumeMessageAsync(CancellationToken.None);

        // Assert
        _kafkaProducerMock.Verify(p => p.ProduceAsync(
            "test-producer-topic",
            It.Is<Message<Null, string>>(m => ValidateFailureMessage(m, paymentId, customerId, "OCR engine failure")),
            It.IsAny<CancellationToken>()), Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error processing check image")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ConsumeMessageAsync_ShouldLogConsumeException()
    {
        // Arrange
        var consumeException = new ConsumeException(
            new ConsumeResult<byte[], byte[]>(),
            new Error(ErrorCode.Local_QueueFull));

        _consumerMock
            .Setup(c => c.Consume(It.IsAny<CancellationToken>()))
            .Throws(consumeException);

        // Act
        await _sut.ConsumeMessageAsync(CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error consuming message")),
                consumeException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _checkReaderMock.Verify(r => r.ReadCheckAsync(It.IsAny<byte[]>()), Times.Never);
    }

    private ConsumeResult<Ignore, string> CreateConsumeResult(string messageValue)
    {
        return new ConsumeResult<Ignore, string>
        {
            Message = new Message<Ignore, string> { Value = messageValue },
            TopicPartitionOffset = new TopicPartitionOffset("test-topic", new Partition(0), new Offset(0))
        };
    }

    private bool ValidateSuccessMessage(Message<Null, string> message, string paymentId, Guid customerId, decimal amount)
    {
        var deserializedStatus = JsonSerializer.Deserialize<PaymentStatus>(message.Value);
        return deserializedStatus != null &&
               deserializedStatus.PaymentId == paymentId &&
               deserializedStatus.CustomerId == customerId &&
               deserializedStatus.Amount == amount &&
               deserializedStatus.Status == "Success" &&
               deserializedStatus.PaymentMethod == "Check";
    }

    private bool ValidateFailureMessage(Message<Null, string> message, string paymentId, Guid customerId, string errorMessage)
    {
        var deserializedStatus = JsonSerializer.Deserialize<PaymentStatus>(message.Value);
        return deserializedStatus != null &&
               deserializedStatus.PaymentId == paymentId &&
               deserializedStatus.CustomerId == customerId &&
               deserializedStatus.Amount == 0 &&
               deserializedStatus.Status == "Failed" &&
               deserializedStatus.ErrorMessage == errorMessage &&
               deserializedStatus.PaymentMethod == "Check";
    }
}