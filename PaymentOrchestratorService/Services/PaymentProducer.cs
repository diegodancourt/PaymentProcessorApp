using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using PaymentOrchestratorService.Configuration;

namespace PaymentOrchestratorService.Services;

public class PaymentProducer : IPaymentProducer, IDisposable
{
    private readonly ILogger<PaymentProducer> _logger;
    private readonly IProducer<Null, string> _producer;
    private readonly KafkaSettings _kafkaSettings;

    public PaymentProducer(
        ILogger<PaymentProducer> logger,
        IOptions<KafkaSettings> kafkaSettings)
    {
        _logger = logger;
        _kafkaSettings = kafkaSettings.Value;

        var config = new ProducerConfig
        {
            BootstrapServers = _kafkaSettings.BootstrapServers,
            Acks = Acks.All,
            MessageTimeoutMs = _kafkaSettings.ProducerTimeoutMs
        };

        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task<string> PublishCheckPaymentAsync(
        Guid customerId,
        byte[] imageData,
        CancellationToken cancellationToken = default)
    {
        var paymentId = GeneratePaymentId();

        var payload = new
        {
            PaymentId = paymentId,
            CustomerId = customerId,
            ImageData = imageData
        };

        var messageJson = JsonSerializer.Serialize(payload);
        var message = new Message<Null, string> { Value = messageJson };

        try
        {
            var result = await _producer.ProduceAsync(
                _kafkaSettings.CheckPaymentTopic,
                message,
                cancellationToken);

            _logger.LogInformation(
                "Published check payment request - PaymentId: {PaymentId}, CustomerId: {CustomerId}, Topic: {Topic}, Partition: {Partition}, Offset: {Offset}",
                paymentId,
                customerId,
                _kafkaSettings.CheckPaymentTopic,
                result.Partition.Value,
                result.Offset.Value);

            return paymentId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish check payment request - PaymentId: {PaymentId}", paymentId);
            throw;
        }
    }

    public async Task<string> PublishCardPaymentAsync(
        Guid customerId,
        string cardNumber,
        string expiryDate,
        string cvv,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        var paymentId = GeneratePaymentId();

        var payload = new
        {
            PaymentId = paymentId,
            CustomerId = customerId,
            CardNumber = cardNumber,
            ExpiryDate = expiryDate,
            Cvv = cvv,
            Amount = amount
        };

        var messageJson = JsonSerializer.Serialize(payload);
        var message = new Message<Null, string> { Value = messageJson };

        try
        {
            var result = await _producer.ProduceAsync(
                _kafkaSettings.CardPaymentTopic,
                message,
                cancellationToken);

            _logger.LogInformation(
                "Published card payment request - PaymentId: {PaymentId}, CustomerId: {CustomerId}, Amount: {Amount}, Topic: {Topic}, Partition: {Partition}, Offset: {Offset}",
                paymentId,
                customerId,
                amount,
                _kafkaSettings.CardPaymentTopic,
                result.Partition.Value,
                result.Offset.Value);

            return paymentId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish card payment request - PaymentId: {PaymentId}", paymentId);
            throw;
        }
    }

    private static string GeneratePaymentId()
    {
        return $"payment-{Guid.NewGuid():N}";
    }

    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
        GC.SuppressFinalize(this);
    }
}