namespace LedgerService.Tests;

public class PaymentStatusTests
{
    [Fact]
    public void PaymentStatus_ShouldBeInitializedWithRequiredProperties()
    {
        // Arrange
        var paymentId = "PAY-12345";
        var customerId = Guid.NewGuid();
        var amount = 150.75m;
        var status = "Success";
        var timestamp = DateTime.UtcNow;

        // Act
        var paymentStatus = new PaymentStatus
        {
            PaymentId = paymentId,
            CustomerId = customerId,
            Amount = amount,
            Status = status,
            Timestamp = timestamp
        };

        // Assert
        paymentStatus.PaymentId.Should().Be(paymentId);
        paymentStatus.CustomerId.Should().Be(customerId);
        paymentStatus.Amount.Should().Be(amount);
        paymentStatus.Status.Should().Be(status);
        paymentStatus.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void PaymentStatus_ErrorMessage_ShouldBeOptional()
    {
        // Arrange & Act
        var paymentStatus = new PaymentStatus
        {
            PaymentId = "PAY-12345",
            CustomerId = Guid.NewGuid(),
            Amount = 150.75m,
            Status = "Failed",
            Timestamp = DateTime.UtcNow,
            ErrorMessage = "Insufficient funds"
        };

        // Assert
        paymentStatus.ErrorMessage.Should().Be("Insufficient funds");
    }

    [Fact]
    public void PaymentStatus_PaymentMethod_ShouldBeOptional()
    {
        // Arrange & Act
        var paymentStatus = new PaymentStatus
        {
            PaymentId = "PAY-12345",
            CustomerId = Guid.NewGuid(),
            Amount = 150.75m,
            Status = "Success",
            Timestamp = DateTime.UtcNow,
            PaymentMethod = "CreditCard"
        };

        // Assert
        paymentStatus.PaymentMethod.Should().Be("CreditCard");
    }

    [Fact]
    public void PaymentStatus_ShouldSupportMultipleStatuses()
    {
        // Arrange
        var statuses = new[] { "Success", "Failed", "Pending", "Cancelled" };

        // Act & Assert
        foreach (var status in statuses)
        {
            var paymentStatus = new PaymentStatus
            {
                PaymentId = "PAY-12345",
                CustomerId = Guid.NewGuid(),
                Amount = 100m,
                Status = status,
                Timestamp = DateTime.UtcNow
            };

            paymentStatus.Status.Should().Be(status);
        }
    }

    [Fact]
    public void PaymentStatus_Amount_ShouldSupportDecimals()
    {
        // Arrange
        var amount = 99.99m;

        // Act
        var paymentStatus = new PaymentStatus
        {
            PaymentId = "PAY-12345",
            CustomerId = Guid.NewGuid(),
            Amount = amount,
            Status = "Success",
            Timestamp = DateTime.UtcNow
        };

        // Assert
        paymentStatus.Amount.Should().Be(amount);
    }

    [Fact]
    public void PaymentStatus_CustomerId_ShouldBeGuid()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var paymentStatus = new PaymentStatus
        {
            PaymentId = "PAY-12345",
            CustomerId = customerId,
            Amount = 100m,
            Status = "Success",
            Timestamp = DateTime.UtcNow
        };

        // Assert
        paymentStatus.CustomerId.Should().Be(customerId);
    }
}
