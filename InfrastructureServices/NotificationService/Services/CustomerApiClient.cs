using NotificationService.Domain;
using Refit;

namespace NotificationService.Services;

public class CustomerApiClient : ICustomerApiClient
{
    private readonly ICustomerApiRefitClient _refitClient;
    private readonly ILogger<CustomerApiClient> _logger;

    public CustomerApiClient(
        ICustomerApiRefitClient refitClient,
        ILogger<CustomerApiClient> logger)
    {
        _refitClient = refitClient;
        _logger = logger;
    }

    public async Task<Customer?> GetCustomerAsync(Guid customerId)
    {
        try
        {
            _logger.LogInformation("Fetching customer data for CustomerId: {CustomerId}", customerId);

            var customer = await _refitClient.GetCustomerAsync(customerId);

            _logger.LogInformation("Successfully fetched customer: {CustomerName} ({CustomerEmail})", customer.Name, customer.Email);
            return customer;
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Customer not found: {CustomerId}", customerId);
            return null;
        }
        catch (ApiException ex)
        {
            _logger.LogError(
                ex,
                "API error while fetching customer. StatusCode: {StatusCode}, CustomerId: {CustomerId}",
                ex.StatusCode,
                customerId);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while fetching customer: {CustomerId}", customerId);
            return null;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request timeout while fetching customer: {CustomerId}", customerId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching customer: {CustomerId}", customerId);
            return null;
        }
    }
}