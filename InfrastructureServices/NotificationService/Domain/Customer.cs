namespace NotificationService.Domain;

public class Customer
{
    public required Guid CustomerId { get; init; }
    public required string Email { get; init; }
    public required string Name { get; init; }
    public string? Phone { get; init; }
}