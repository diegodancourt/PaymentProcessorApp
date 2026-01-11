using NotificationService.Domain;

namespace NotificationService.Tests;

public class PaymentStatusTests
{
    [Fact]
    public void PaymentStatus_ShouldInitializeWithRequiredProperties()
    {
        var customerId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;
        var paymentStatus = new PaymentStatus
        {
            PaymentId = "PAY-123",
            CustomerId = customerId,
            Amount = 150.00m,
            Status = "Success",
            Timestamp = timestamp
        };

        Assert.Equal("PAY-123", paymentStatus.PaymentId);
        Assert.Equal(customerId, paymentStatus.CustomerId);
        Assert.Equal(150.00m, paymentStatus.Amount);
        Assert.Equal("Success", paymentStatus.Status);
        Assert.Equal(timestamp, paymentStatus.Timestamp);
    }

    [Fact]
    public void PaymentStatus_ShouldAllowOptionalErrorMessage()
    {
        var paymentStatus = new PaymentStatus
        {
            PaymentId = "PAY-456",
            CustomerId = Guid.NewGuid(),
            Amount = 200.00m,
            Status = "Failed",
            Timestamp = DateTime.UtcNow,
            ErrorMessage = "Insufficient funds"
        };

        Assert.Equal("Insufficient funds", paymentStatus.ErrorMessage);
    }

    [Fact]
    public void PaymentStatus_ShouldAllowOptionalPaymentMethod()
    {
        var paymentStatus = new PaymentStatus
        {
            PaymentId = "PAY-789",
            CustomerId = Guid.NewGuid(),
            Amount = 300.00m,
            Status = "Success",
            Timestamp = DateTime.UtcNow,
            PaymentMethod = "Check"
        };

        Assert.Equal("Check", paymentStatus.PaymentMethod);
    }

    [Fact]
    public void PaymentStatus_ShouldAllowNullOptionalProperties()
    {
        var paymentStatus = new PaymentStatus
        {
            PaymentId = "PAY-999",
            CustomerId = Guid.NewGuid(),
            Amount = 100.00m,
            Status = "Pending",
            Timestamp = DateTime.UtcNow
        };

        Assert.Null(paymentStatus.ErrorMessage);
        Assert.Null(paymentStatus.PaymentMethod);
    }

    [Theory]
    [InlineData("Success")]
    [InlineData("Failed")]
    [InlineData("Pending")]
    public void PaymentStatus_ShouldAcceptDifferentStatuses(string status)
    {
        var paymentStatus = new PaymentStatus
        {
            PaymentId = "PAY-111",
            CustomerId = Guid.NewGuid(),
            Amount = 50.00m,
            Status = status,
            Timestamp = DateTime.UtcNow
        };

        Assert.Equal(status, paymentStatus.Status);
    }
}