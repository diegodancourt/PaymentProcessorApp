using NotificationService.Domain;
using Refit;

namespace NotificationService.Services;

public interface ICustomerApiRefitClient
{
    [Get("/api/customers/{customerId}")]
    Task<Customer> GetCustomerAsync(Guid customerId);
}