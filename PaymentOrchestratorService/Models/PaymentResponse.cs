namespace PaymentOrchestratorService.Models;

public class PaymentResponse
{
    public required string PaymentId { get; init; }
    public required string Status { get; init; }
    public required string Message { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}