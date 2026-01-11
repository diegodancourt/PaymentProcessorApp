using CustomerApi.Models;

namespace CustomerApi.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid customerId);
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<Customer> CreateAsync(Customer customer);
    Task<bool> UpdateAsync(Customer customer);
    Task<bool> DeleteAsync(Guid customerId);
}