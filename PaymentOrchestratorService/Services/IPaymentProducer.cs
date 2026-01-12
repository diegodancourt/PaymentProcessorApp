namespace PaymentOrchestratorService.Services;

public interface IPaymentProducer
{
    Task<string> PublishCheckPaymentAsync(Guid customerId, byte[] imageData, CancellationToken cancellationToken = default);
    Task<string> PublishCardPaymentAsync(Guid customerId, string cardNumber, string expiryDate, string cvv, decimal amount, CancellationToken cancellationToken = default);
}