namespace CheckService.Domain;

public class PaymentStatus
{
    public required string PaymentId { get; init; }
    public required Guid CustomerId { get; init; }
    public required decimal Amount { get; init; }
    public required string Status { get; init; } // "Success", "Failed", "Pending"
    public string? ErrorMessage { get; init; }
    public required DateTime Timestamp { get; init; }
    public string? PaymentMethod { get; init; } // "Check", "Card"
}
