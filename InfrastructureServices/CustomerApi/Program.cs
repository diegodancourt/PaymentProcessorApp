using CustomerApi.Models;
using CustomerApi.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// Register repository
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// GET: Retrieve customer by ID
app.MapGet("/api/customers/{customerId:guid}", async (Guid customerId, ICustomerRepository repository) =>
{
    var customer = await repository.GetByIdAsync(customerId);

    if (customer == null)
    {
        return Results.NotFound(new { message = "Customer not found" });
    }

    return Results.Ok(customer);
})
.WithName("GetCustomer");

// GET: Retrieve all customers
app.MapGet("/api/customers", async (ICustomerRepository repository) =>
{
    var customers = await repository.GetAllAsync();
    return Results.Ok(customers);
})
.WithName("GetAllCustomers");

// POST: Create a new customer
app.MapPost("/api/customers", async (Customer customer, ICustomerRepository repository) =>
{
    try
    {
        var createdCustomer = await repository.CreateAsync(customer);
        return Results.Created($"/api/customers/{createdCustomer.CustomerId}", createdCustomer);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
    }
})
.WithName("CreateCustomer");

// PUT: Update an existing customer
app.MapPut("/api/customers/{customerId:guid}", async (Guid customerId, Customer customer, ICustomerRepository repository) =>
{
    if (customerId != customer.CustomerId)
    {
        return Results.BadRequest(new { message = "Customer ID mismatch" });
    }

    var updated = await repository.UpdateAsync(customer);

    if (!updated)
    {
        return Results.NotFound(new { message = "Customer not found" });
    }

    return Results.NoContent();
})
.WithName("UpdateCustomer");

// DELETE: Delete a customer
app.MapDelete("/api/customers/{customerId:guid}", async (Guid customerId, ICustomerRepository repository) =>
{
    var deleted = await repository.DeleteAsync(customerId);

    if (!deleted)
    {
        return Results.NotFound(new { message = "Customer not found" });
    }

    return Results.NoContent();
})
.WithName("DeleteCustomer");

app.Run();