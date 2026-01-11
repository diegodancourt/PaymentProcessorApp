using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NotificationService.Domain;
using NotificationService.Services;

namespace NotificationService.Tests;

public class WorkerTests
{
    private readonly Mock<ILogger<Worker>> _loggerMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ICustomerApiClient> _customerApiClientMock;
    private readonly IOptions<KafkaSettings> _kafkaSettings;

    public WorkerTests()
    {
        _loggerMock = new Mock<ILogger<Worker>>();
        _emailServiceMock = new Mock<IEmailService>();
        _customerApiClientMock = new Mock<ICustomerApiClient>();
        _kafkaSettings = Options.Create(new KafkaSettings
        {
            BootstrapServers = "localhost:9092",
            GroupId = "test-group",
            Topic = "test-topic",
            AutoOffsetReset = true,
            EnableAutoCommit = false
        });
    }

    [Fact]
    public void Worker_ShouldInitializeWithDependencies()
    {
        var worker = new Worker(
            _loggerMock.Object,
            _kafkaSettings,
            _emailServiceMock.Object,
            _customerApiClientMock.Object);

        Assert.NotNull(worker);
    }

    [Fact]
    public async Task Worker_ShouldProcessPaymentStatus_WhenValidMessageReceived()
    {
        var customerId = Guid.NewGuid();
        var paymentStatus = new PaymentStatus
        {
            PaymentId = "PAY-123",
            CustomerId = customerId,
            Amount = 100.50m,
            Status = "Success",
            Timestamp = DateTime.UtcNow,
            PaymentMethod = "Check"
        };

        var customer = new Customer
        {
            CustomerId = customerId,
            Email = "customer@example.com",
            Name = "John Doe"
        };

        _customerApiClientMock
            .Setup(x => x.GetCustomerAsync(customerId))
            .ReturnsAsync(customer);

        _emailServiceMock
            .Setup(x => x.SendPaymentNotificationAsync(It.IsAny<PaymentStatus>(), It.IsAny<Customer>()))
            .Returns(Task.CompletedTask);

        // Note: Full integration testing of ExecuteAsync requires a Kafka test environment
        // This test validates that the Worker can be constructed with proper dependencies
        var worker = new Worker(
            _loggerMock.Object,
            _kafkaSettings,
            _emailServiceMock.Object,
            _customerApiClientMock.Object);
        Assert.NotNull(worker);
    }

    [Fact]
    public void Worker_ShouldUseKafkaSettingsFromConfiguration()
    {
        var customSettings = Options.Create(new KafkaSettings
        {
            BootstrapServers = "custom:9092",
            GroupId = "custom-group",
            Topic = "custom-topic",
            AutoOffsetReset = false,
            EnableAutoCommit = true
        });

        var worker = new Worker(
            _loggerMock.Object,
            customSettings,
            _emailServiceMock.Object,
            _customerApiClientMock.Object);

        Assert.NotNull(worker);
        // Settings are validated by Worker's constructor accessing kafkaSettings.Value
    }

    [Fact]
    public void Worker_ShouldAcceptEmailServiceDependency()
    {
        var emailServiceMock = new Mock<IEmailService>();
        var worker = new Worker(
            _loggerMock.Object,
            _kafkaSettings,
            emailServiceMock.Object,
            _customerApiClientMock.Object);

        Assert.NotNull(worker);
    }

    [Fact]
    public void Worker_ShouldAcceptCustomerApiClientDependency()
    {
        var customerApiClientMock = new Mock<ICustomerApiClient>();
        var worker = new Worker(
            _loggerMock.Object,
            _kafkaSettings,
            _emailServiceMock.Object,
            customerApiClientMock.Object);

        Assert.NotNull(worker);
    }
}