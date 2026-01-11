namespace CardService.Domain;

public class CardPaymentRequest
{
    public required string PaymentId { get; init; }
    public required Guid CustomerId { get; init; }
    public required decimal Amount { get; init; }
    public required string CardToken { get; init; }
}
