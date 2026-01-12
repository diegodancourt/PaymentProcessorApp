namespace PaymentOrchestratorService.Models;

public class CheckPaymentRequest
{
    public required Guid CustomerId { get; init; }
    public required byte[] ImageData { get; init; }
}