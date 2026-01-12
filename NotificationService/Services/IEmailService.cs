using NotificationService.Domain;

namespace NotificationService.Services;

public interface IEmailService
{
    Task SendPaymentNotificationAsync(PaymentStatus paymentStatus, Customer customer);
}