using Microsoft.Extensions.Options;
using NotificationService.Domain;

namespace NotificationService.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailSettings _emailSettings;

    public EmailService(ILogger<EmailService> logger, IOptions<EmailSettings> emailSettings)
    {
        _logger = logger;
        _emailSettings = emailSettings.Value;
    }

    public async Task SendPaymentNotificationAsync(PaymentStatus paymentStatus, Customer customer)
    {
        // In a real implementation, this would use SMTP to send emails
        // For now, we'll just log the email content

        var subject = paymentStatus.Status switch
        {
            "Success" => $"Payment Successful - {paymentStatus.PaymentId}",
            "Failed" => $"Payment Failed - {paymentStatus.PaymentId}",
            "Pending" => $"Payment Pending - {paymentStatus.PaymentId}",
            _ => $"Payment Status Update - {paymentStatus.PaymentId}"
        };

        var body = GenerateEmailBody(paymentStatus, customer);

        _logger.LogInformation(
            "Sending email to {Email} - Subject: {Subject}",
            customer.Email,
            subject);

        // Simulate email sending
        await Task.Delay(100);

        _logger.LogInformation(
            """

            ===========================================
            EMAIL NOTIFICATION
            ===========================================
            To: {Email}
            Name: {Name}
            Subject: {Subject}

            {Body}
            ===========================================

            """,
            customer.Email,
            customer.Name,
            subject,
            body);

        _logger.LogInformation("Email sent successfully to {Email}", customer.Email);
    }

    private string GenerateEmailBody(PaymentStatus paymentStatus, Customer customer)
    {
        return paymentStatus.Status switch
        {
            "Success" => $"""
                Dear {customer.Name},

                Your payment has been processed successfully!

                Payment Details:
                - Payment ID: {paymentStatus.PaymentId}
                - Amount: ${paymentStatus.Amount:F2}
                - Payment Method: {paymentStatus.PaymentMethod ?? "N/A"}
                - Date: {paymentStatus.Timestamp:yyyy-MM-dd HH:mm:ss}

                Thank you for your payment.

                Best regards,
                Payment Processing Team
                """,

            "Failed" => $"""
                Dear {customer.Name},

                Unfortunately, your payment could not be processed.

                Payment Details:
                - Payment ID: {paymentStatus.PaymentId}
                - Amount: ${paymentStatus.Amount:F2}
                - Payment Method: {paymentStatus.PaymentMethod ?? "N/A"}
                - Date: {paymentStatus.Timestamp:yyyy-MM-dd HH:mm:ss}
                - Reason: {paymentStatus.ErrorMessage ?? "Unknown error"}

                Please try again or contact support for assistance.

                Best regards,
                Payment Processing Team
                """,

            "Pending" => $"""
                Dear {customer.Name},

                Your payment is currently being processed.

                Payment Details:
                - Payment ID: {paymentStatus.PaymentId}
                - Amount: ${paymentStatus.Amount:F2}
                - Payment Method: {paymentStatus.PaymentMethod ?? "N/A"}
                - Date: {paymentStatus.Timestamp:yyyy-MM-dd HH:mm:ss}

                We will notify you once the payment is complete.

                Best regards,
                Payment Processing Team
                """,

            _ => $"""
                Dear {customer.Name},

                Payment Status Update:
                - Payment ID: {paymentStatus.PaymentId}
                - Amount: ${paymentStatus.Amount:F2}
                - Status: {paymentStatus.Status}
                - Date: {paymentStatus.Timestamp:yyyy-MM-dd HH:mm:ss}

                Best regards,
                Payment Processing Team
                """
        };
    }
}