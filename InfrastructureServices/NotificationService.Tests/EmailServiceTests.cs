using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NotificationService.Domain;
using NotificationService.Services;

namespace NotificationService.Tests;

public class EmailServiceTests
{
    private readonly Mock<ILogger<EmailService>> _loggerMock;
    private readonly IOptions<EmailSettings> _emailSettings;
    private readonly EmailService _emailService;

    public EmailServiceTests()
    {
        _loggerMock = new Mock<ILogger<EmailService>>();
        _emailSettings = Options.Create(new EmailSettings
        {
            SmtpServer = "smtp.test.com",
            SmtpPort = 587,
            FromEmail = "test@example.com",
            FromName = "Test Sender",
            EnableSsl = true
        });
        _emailService = new EmailService(_loggerMock.Object, _emailSettings);
    }

    [Fact]
    public void EmailService_ShouldInitializeWithDependencies()
    {
        Assert.NotNull(_emailService);
    }

    [Fact]
    public async Task SendPaymentNotificationAsync_ShouldCompleteSuccessfully_ForSuccessStatus()
    {
        var paymentStatus = new PaymentStatus
        {
            PaymentId = "PAY-123",
            CustomerId = Guid.NewGuid(),
            Amount = 150.00m,
            Status = "Success",
            Timestamp = DateTime.UtcNow,
            PaymentMethod = "Check"
        };

        var customer = new Customer
        {
            CustomerId = paymentStatus.CustomerId,
            Email = "customer@example.com",
            Name = "John Doe"
        };

        await _emailService.SendPaymentNotificationAsync(paymentStatus, customer);

        // Verify logging occurred
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Email sent successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPaymentNotificationAsync_ShouldCompleteSuccessfully_ForFailedStatus()
    {
        var paymentStatus = new PaymentStatus
        {
            PaymentId = "PAY-456",
            CustomerId = Guid.NewGuid(),
            Amount = 200.00m,
            Status = "Failed",
            ErrorMessage = "Insufficient funds",
            Timestamp = DateTime.UtcNow,
            PaymentMethod = "Card"
        };

        var customer = new Customer
        {
            CustomerId = paymentStatus.CustomerId,
            Email = "customer@example.com",
            Name = "Jane Smith"
        };

        await _emailService.SendPaymentNotificationAsync(paymentStatus, customer);

        // Verify logging occurred
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Email sent successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPaymentNotificationAsync_ShouldCompleteSuccessfully_ForPendingStatus()
    {
        var paymentStatus = new PaymentStatus
        {
            PaymentId = "PAY-789",
            CustomerId = Guid.NewGuid(),
            Amount = 300.00m,
            Status = "Pending",
            Timestamp = DateTime.UtcNow,
            PaymentMethod = "Check"
        };

        var customer = new Customer
        {
            CustomerId = paymentStatus.CustomerId,
            Email = "customer@example.com",
            Name = "Bob Johnson"
        };

        await _emailService.SendPaymentNotificationAsync(paymentStatus, customer);

        // Verify logging occurred
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Email sent successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPaymentNotificationAsync_ShouldHandleUnknownStatus()
    {
        var paymentStatus = new PaymentStatus
        {
            PaymentId = "PAY-999",
            CustomerId = Guid.NewGuid(),
            Amount = 100.00m,
            Status = "Unknown",
            Timestamp = DateTime.UtcNow
        };

        var customer = new Customer
        {
            CustomerId = paymentStatus.CustomerId,
            Email = "customer@example.com",
            Name = "Alice Brown"
        };

        await _emailService.SendPaymentNotificationAsync(paymentStatus, customer);

        // Verify logging occurred
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Email sent successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPaymentNotificationAsync_ShouldLogSendingEmail()
    {
        var paymentStatus = new PaymentStatus
        {
            PaymentId = "PAY-111",
            CustomerId = Guid.NewGuid(),
            Amount = 50.00m,
            Status = "Success",
            Timestamp = DateTime.UtcNow
        };

        var customer = new Customer
        {
            CustomerId = paymentStatus.CustomerId,
            Email = "test@example.com",
            Name = "Test User"
        };

        await _emailService.SendPaymentNotificationAsync(paymentStatus, customer);

        // Verify that email sending was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Sending email")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}