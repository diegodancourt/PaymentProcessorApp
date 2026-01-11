using LedgerService.Domain;

namespace LedgerService.Services;

public interface ILedgerRepository
{
    Task SavePaymentStatusAsync(PaymentStatus paymentStatus);
    Task<PaymentStatus?> GetPaymentStatusAsync(string paymentId);
}