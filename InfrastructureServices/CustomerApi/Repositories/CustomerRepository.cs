using System.Data;
using CustomerApi.Models;
using Dapper;
using Npgsql;

namespace CustomerApi.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly string _connectionString;
    private readonly ILogger<CustomerRepository> _logger;

    public CustomerRepository(IConfiguration configuration, ILogger<CustomerRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("PostgresConnection")
            ?? throw new InvalidOperationException("PostgresConnection connection string is not configured");
        _logger = logger;
    }

    private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    public async Task<Customer?> GetByIdAsync(Guid customerId)
    {
        const string sql = @"
            SELECT customer_id AS CustomerId,
                   email AS Email,
                   name AS Name,
                   phone AS Phone
            FROM customers
            WHERE customer_id = @CustomerId";

        try
        {
            using var connection = CreateConnection();
            var customer = await connection.QuerySingleOrDefaultAsync<Customer>(sql, new { CustomerId = customerId });
            return customer;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer {CustomerId}", customerId);
            throw;
        }
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        const string sql = @"
            SELECT customer_id AS CustomerId,
                   email AS Email,
                   name AS Name,
                   phone AS Phone
            FROM customers
            ORDER BY name";

        try
        {
            using var connection = CreateConnection();
            var customers = await connection.QueryAsync<Customer>(sql);
            return customers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all customers");
            throw;
        }
    }

    public async Task<Customer> CreateAsync(Customer customer)
    {
        const string sql = @"
            INSERT INTO customers (customer_id, email, name, phone)
            VALUES (@CustomerId, @Email, @Name, @Phone)
            RETURNING customer_id AS CustomerId,
                      email AS Email,
                      name AS Name,
                      phone AS Phone";

        try
        {
            using var connection = CreateConnection();
            var createdCustomer = await connection.QuerySingleAsync<Customer>(sql, customer);
            _logger.LogInformation("Created customer {CustomerId}", createdCustomer.CustomerId);
            return createdCustomer;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer {CustomerId}", customer.CustomerId);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(Customer customer)
    {
        const string sql = @"
            UPDATE customers
            SET email = @Email,
                name = @Name,
                phone = @Phone
            WHERE customer_id = @CustomerId";

        try
        {
            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(sql, customer);

            if (affectedRows > 0)
            {
                _logger.LogInformation("Updated customer {CustomerId}", customer.CustomerId);
            }

            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer {CustomerId}", customer.CustomerId);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid customerId)
    {
        const string sql = "DELETE FROM customers WHERE customer_id = @CustomerId";

        try
        {
            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(sql, new { CustomerId = customerId });

            if (affectedRows > 0)
            {
                _logger.LogInformation("Deleted customer {CustomerId}", customerId);
            }

            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer {CustomerId}", customerId);
            throw;
        }
    }
}