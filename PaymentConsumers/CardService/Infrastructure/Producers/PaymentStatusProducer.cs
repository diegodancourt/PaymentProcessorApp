using System.Text.Json;
using CardService.Domain;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace CardService.Infrastructure.Producers
{
    public class PaymentStatusProducer
    {
        private readonly ILogger<PaymentStatusProducer> _logger;
        private readonly IProducer<Null, string> _producer;
        private readonly KafkaSettings _kafkaSettings;

        public PaymentStatusProducer(ILogger<PaymentStatusProducer> logger, IProducer<Null, string> producer,
            IOptions<KafkaSettings> kafkaSettings)
        {
            _logger = logger;
            _producer = producer;
            _kafkaSettings = kafkaSettings.Value;
        }

        public async Task PublishPaymentStatusAsync(
            PaymentStatus paymentStatus,
            CancellationToken stoppingToken)
        {
            try
            {
                var messageJson = JsonSerializer.Serialize(paymentStatus);
                var message = new Message<Null, string> { Value = messageJson };

                var result = await _producer.ProduceAsync(_kafkaSettings.ProducerTopic, message, stoppingToken);

                _logger.LogInformation(
                    "Published payment status - PaymentId: {PaymentId}, Status: {Status}, Topic: {Topic}, Partition: {Partition}, Offset: {Offset}",
                    paymentStatus.PaymentId,
                    paymentStatus.Status,
                    _kafkaSettings.ProducerTopic,
                    result.Partition.Value,
                    result.Offset.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish payment status for PaymentId: {PaymentId}",
                    paymentStatus.PaymentId);
                throw;
            }
        }

        public async Task PublishFailureStatusAsync(
            CardPaymentRequest request,
            string errorMessage,
            CancellationToken stoppingToken)
        {
            try
            {
                var paymentStatus = new PaymentStatus
                {
                    PaymentId = request.PaymentId,
                    CustomerId = request.CustomerId,
                    Amount = 0,
                    Status = "Failed",
                    ErrorMessage = errorMessage,
                    Timestamp = DateTime.UtcNow,
                    PaymentMethod = "Check"
                };

                await PublishPaymentStatusAsync(paymentStatus, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish failure status for PaymentId: {PaymentId}", request.PaymentId);
            }
        }
    }
}