namespace PaymentOrchestratorService.Models;

public class CardPaymentRequest
{
    public required Guid CustomerId { get; init; }
    public required string CardNumber { get; init; }
    public required string ExpiryDate { get; init; }
    public required string Cvv { get; init; }
    public required decimal Amount { get; init; }
}