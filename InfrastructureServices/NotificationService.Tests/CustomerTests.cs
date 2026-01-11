using NotificationService.Domain;

namespace NotificationService.Tests;

public class CustomerTests
{
    [Fact]
    public void Customer_ShouldInitializeWithRequiredProperties()
    {
        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            CustomerId = customerId,
            Email = "test@example.com",
            Name = "Test User"
        };

        Assert.Equal(customerId, customer.CustomerId);
        Assert.Equal("test@example.com", customer.Email);
        Assert.Equal("Test User", customer.Name);
    }

    [Fact]
    public void Customer_ShouldAllowOptionalPhone()
    {
        var customer = new Customer
        {
            CustomerId = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
            Phone = "+1-555-0100"
        };

        Assert.Equal("+1-555-0100", customer.Phone);
    }

    [Fact]
    public void Customer_ShouldAllowNullPhone()
    {
        var customer = new Customer
        {
            CustomerId = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User"
        };

        Assert.Null(customer.Phone);
    }
}