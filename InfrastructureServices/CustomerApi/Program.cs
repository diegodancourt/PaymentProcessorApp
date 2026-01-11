using CustomerApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// In-memory customer database (for demo purposes)
var customers = new Dictionary<Guid, Customer>
{
    {
        Guid.Parse("11111111-1111-1111-1111-111111111111"),
        new Customer
        {
            CustomerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Email = "john.doe@example.com",
            Name = "John Doe",
            Phone = "+1-555-0101"
        }
    },
    {
        Guid.Parse("22222222-2222-2222-2222-222222222222"),
        new Customer
        {
            CustomerId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Email = "jane.smith@example.com",
            Name = "Jane Smith",
            Phone = "+1-555-0102"
        }
    },
    {
        Guid.Parse("33333333-3333-3333-3333-333333333333"),
        new Customer
        {
            CustomerId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Email = "bob.johnson@example.com",
            Name = "Bob Johnson",
            Phone = "+1-555-0103"
        }
    }
};

app.MapGet("/api/customers/{customerId:guid}", (Guid customerId) =>
{
    if (customers.TryGetValue(customerId, out var customer))
    {
        return Results.Ok(customer);
    }

    return Results.NotFound(new { message = "Customer not found" });
})
.WithName("GetCustomer");

app.Run();