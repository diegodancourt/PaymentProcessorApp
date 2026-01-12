using NotificationService.Domain;

namespace NotificationService.Services;

public interface ICustomerApiClient
{
    Task<Customer?> GetCustomerAsync(Guid customerId);
}