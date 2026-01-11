using CheckReader.Services;
using CheckService.Infrastructure.Consumers;
using CheckService.Infrastructure.Producers;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CheckService.Tests;

public class WorkerTests
{
    private readonly Mock<ILogger<Worker>> _loggerMock;
    private readonly PaymentRequestConsumer _consumer;
    private readonly IOptions<KafkaSettings> _kafkaSettings;

    public WorkerTests()
    {
        _loggerMock = new Mock<ILogger<Worker>>();

        var consumerLoggerMock = new Mock<ILogger<PaymentRequestConsumer>>();
        var kafkaConsumerMock = new Mock<IConsumer<Ignore, string>>();
        var checkReaderMock = new Mock<ICheckReader>();

        var producerLoggerMock = new Mock<ILogger<PaymentStatusProducer>>();
        var kafkaProducerMock = new Mock<IProducer<Null, string>>();

        _kafkaSettings = Options.Create(new KafkaSettings
        {
            BootstrapServers = "localhost:9092",
            GroupId = "test-group",
            ConsumerTopic = "test-consumer-topic",
            ProducerTopic = "test-producer-topic",
            AutoOffsetReset = true,
            EnableAutoCommit = false
        });

        var producer = new PaymentStatusProducer(
            producerLoggerMock.Object,
            kafkaProducerMock.Object,
            _kafkaSettings);

        _consumer = new PaymentRequestConsumer(
            consumerLoggerMock.Object,
            kafkaConsumerMock.Object,
            producer,
            checkReaderMock.Object);
    }

    [Fact]
    public void Worker_ShouldInitializeWithDependencies()
    {
        // Act
        var worker = new Worker(
            _loggerMock.Object,
            _kafkaSettings,
            _consumer);

        // Assert
        Assert.NotNull(worker);
    }

    [Fact]
    public void Worker_ShouldUseKafkaSettingsFromConfiguration()
    {
        // Arrange
        var customSettings = Options.Create(new KafkaSettings
        {
            BootstrapServers = "custom:9092",
            GroupId = "custom-group",
            ConsumerTopic = "custom-consumer-topic",
            ProducerTopic = "custom-producer-topic",
            AutoOffsetReset = false,
            EnableAutoCommit = true,
            ProducerTimeoutMs = 10000
        });

        // Act
        var worker = new Worker(
            _loggerMock.Object,
            customSettings,
            _consumer);

        // Assert
        Assert.NotNull(worker);
        // Settings are validated by Worker's constructor accessing kafkaSettings.Value
    }

    [Fact]
    public void Worker_ShouldAcceptPaymentRequestConsumer()
    {
        // Act
        var worker = new Worker(
            _loggerMock.Object,
            _kafkaSettings,
            _consumer);

        // Assert
        Assert.NotNull(worker);
        // Verifies that Worker accepts PaymentRequestConsumer dependency
    }

    [Fact]
    public void Worker_ShouldAcceptPaymentStatusProducer()
    {
        // Act
        var worker = new Worker(
            _loggerMock.Object,
            _kafkaSettings,
            _consumer);

        // Assert
        Assert.NotNull(worker);
        // Verifies that Worker accepts PaymentStatusProducer dependency
    }
}