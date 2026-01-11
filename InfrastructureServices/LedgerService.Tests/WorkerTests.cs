using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LedgerService.Tests;

public class WorkerTests
{
    private readonly Mock<ILogger<Worker>> _mockLogger;
    private readonly Mock<ILedgerRepository> _mockLedgerRepository;
    private readonly KafkaSettings _kafkaSettings;

    public WorkerTests()
    {
        _mockLogger = new Mock<ILogger<Worker>>();
        _mockLedgerRepository = new Mock<ILedgerRepository>();
        _kafkaSettings = new KafkaSettings
        {
            BootstrapServers = "localhost:9092",
            GroupId = "ledger-service-test",
            Topic = "payment-status-test",
            AutoOffsetReset = true,
            EnableAutoCommit = false
        };
    }

    [Fact]
    public void Worker_ShouldBeInitializedWithDependencies()
    {
        // Arrange & Act
        var worker = new Worker(
            _mockLogger.Object,
            Options.Create(_kafkaSettings),
            _mockLedgerRepository.Object
        );

        // Assert
        worker.Should().NotBeNull();
    }

    [Fact]
    public void Worker_ShouldAcceptKafkaSettings()
    {
        // Arrange
        var settings = new KafkaSettings
        {
            BootstrapServers = "localhost:9092",
            GroupId = "test-group",
            Topic = "test-topic",
            AutoOffsetReset = true,
            EnableAutoCommit = false
        };

        // Act
        var worker = new Worker(
            _mockLogger.Object,
            Options.Create(settings),
            _mockLedgerRepository.Object
        );

        // Assert
        worker.Should().NotBeNull();
    }

    [Fact]
    public void Worker_ShouldAcceptLedgerRepository()
    {
        // Arrange
        var mockRepository = new Mock<ILedgerRepository>();

        // Act
        var worker = new Worker(
            _mockLogger.Object,
            Options.Create(_kafkaSettings),
            mockRepository.Object
        );

        // Assert
        worker.Should().NotBeNull();
    }
}
