namespace CheckService.Domain;

public class CheckPaymentRequest
{
    public required string PaymentId { get; init; }
    public required Guid CustomerId { get; init; }
    public required byte[] ImageData { get; init; }
}
