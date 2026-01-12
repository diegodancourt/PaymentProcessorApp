using Microsoft.Extensions.Logging;
using Moq;
using NotificationService.Domain;
using NotificationService.Services;
using Refit;

namespace NotificationService.Tests;

public class CustomerApiClientTests
{
    private readonly Mock<ICustomerApiRefitClient> _refitClientMock;
    private readonly Mock<ILogger<CustomerApiClient>> _loggerMock;
    private readonly CustomerApiClient _customerApiClient;

    public CustomerApiClientTests()
    {
        _refitClientMock = new Mock<ICustomerApiRefitClient>();
        _loggerMock = new Mock<ILogger<CustomerApiClient>>();
        _customerApiClient = new CustomerApiClient(_refitClientMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetCustomerAsync_ShouldReturnCustomer_WhenCustomerExists()
    {
        var customerId = Guid.NewGuid();
        var expectedCustomer = new Customer
        {
            CustomerId = customerId,
            Email = "test@example.com",
            Name = "Test User",
            Phone = "+1-555-0100"
        };

        _refitClientMock
            .Setup(x => x.GetCustomerAsync(customerId))
            .ReturnsAsync(expectedCustomer);

        var result = await _customerApiClient.GetCustomerAsync(customerId);

        Assert.NotNull(result);
        Assert.Equal(customerId, result.CustomerId);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("Test User", result.Name);
        Assert.Equal("+1-555-0100", result.Phone);
    }

    [Fact]
    public async Task GetCustomerAsync_ShouldReturnNull_WhenCustomerNotFound()
    {
        var customerId = Guid.NewGuid();

        _refitClientMock
            .Setup(x => x.GetCustomerAsync(customerId))
            .ThrowsAsync(await ApiException.Create(
                new HttpRequestMessage(),
                HttpMethod.Get,
                new HttpResponseMessage(System.Net.HttpStatusCode.NotFound),
                new RefitSettings()));

        var result = await _customerApiClient.GetCustomerAsync(customerId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCustomerAsync_ShouldReturnNull_WhenApiExceptionOccurs()
    {
        var customerId = Guid.NewGuid();

        _refitClientMock
            .Setup(x => x.GetCustomerAsync(customerId))
            .ThrowsAsync(await ApiException.Create(
                new HttpRequestMessage(),
                HttpMethod.Get,
                new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError),
                new RefitSettings()));

        var result = await _customerApiClient.GetCustomerAsync(customerId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCustomerAsync_ShouldReturnNull_WhenHttpRequestExceptionOccurs()
    {
        var customerId = Guid.NewGuid();

        _refitClientMock
            .Setup(x => x.GetCustomerAsync(customerId))
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        var result = await _customerApiClient.GetCustomerAsync(customerId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCustomerAsync_ShouldReturnNull_WhenTimeoutOccurs()
    {
        var customerId = Guid.NewGuid();

        _refitClientMock
            .Setup(x => x.GetCustomerAsync(customerId))
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        var result = await _customerApiClient.GetCustomerAsync(customerId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCustomerAsync_ShouldLogInformation_WhenSuccessful()
    {
        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            CustomerId = customerId,
            Email = "test@example.com",
            Name = "Test User"
        };

        _refitClientMock
            .Setup(x => x.GetCustomerAsync(customerId))
            .ReturnsAsync(customer);

        await _customerApiClient.GetCustomerAsync(customerId);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fetching customer data")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully fetched customer")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCustomerAsync_ShouldLogWarning_WhenCustomerNotFound()
    {
        var customerId = Guid.NewGuid();

        _refitClientMock
            .Setup(x => x.GetCustomerAsync(customerId))
            .ThrowsAsync(await ApiException.Create(
                new HttpRequestMessage(),
                HttpMethod.Get,
                new HttpResponseMessage(System.Net.HttpStatusCode.NotFound),
                new RefitSettings()));

        await _customerApiClient.GetCustomerAsync(customerId);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Customer not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}