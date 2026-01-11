using System.Text.Json;
using CheckReader.Services;
using CheckService.Domain;
using CheckService.Infrastructure.Producers;
using Confluent.Kafka;

namespace CheckService.Infrastructure.Consumers
{
    public class PaymentRequestConsumer
    {
        private readonly ILogger<PaymentRequestConsumer> _logger;
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly PaymentStatusProducer _paymentStatusProducer;
        private readonly ICheckReader _checkReader;

        public PaymentRequestConsumer(ILogger<PaymentRequestConsumer> logger, IConsumer<Ignore, string> consumer,
            PaymentStatusProducer paymentStatusProducer, ICheckReader checkReader)
        {
            _logger = logger;
            _consumer = consumer;
            _paymentStatusProducer = paymentStatusProducer;
            _checkReader = checkReader;
        }

        public async Task ConsumeMessageAsync(CancellationToken stoppingToken)
        {
            CheckPaymentRequest? request = null;
            ConsumeResult<Ignore, string>? consumeResult = null;

            try
            {
                consumeResult = _consumer.Consume(stoppingToken);

                if (consumeResult?.Message?.Value == null)
                {
                    return;
                }

                var messageJson = consumeResult.Message.Value;
                _logger.LogInformation(
                    "Received check payment request. Partition: {Partition}, Offset: {Offset}",
                    consumeResult.Partition.Value,
                    consumeResult.Offset.Value);

                request = JsonSerializer.Deserialize<CheckPaymentRequest>(messageJson);

                if (request == null)
                {
                    _logger.LogWarning("Failed to deserialize check payment request");
                    return;
                }

                _logger.LogInformation(
                    "Processing check payment - PaymentId: {PaymentId}, CustomerId: {CustomerId}, ImageSize: {Size} bytes",
                    request.PaymentId,
                    request.CustomerId,
                    request.ImageData.Length);

                var check = await _checkReader.ReadCheckAsync(request.ImageData);

                _logger.LogInformation(
                    "Check processed - PaymentId: {PaymentId}, CheckNumber: {CheckNumber}, Amount: {Amount}, Payee: {Payee}",
                    request.PaymentId,
                    check.Micr.CheckNumber,
                    check.Amount.Value,
                    check.Payee.Name);

                //TODO: Send check data to external payment processor API

                // Create and publish PaymentStatus
                var paymentStatus = new PaymentStatus
                {
                    PaymentId = request.PaymentId,
                    CustomerId = request.CustomerId,
                    Amount = check.Amount.Value,
                    Status = "Success",
                    Timestamp = DateTime.UtcNow,
                    PaymentMethod = "Check"
                };

                await _paymentStatusProducer.PublishPaymentStatusAsync(paymentStatus, stoppingToken);

                _consumer.Commit(consumeResult);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Error consuming message: {Reason}", ex.Error.Reason);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing check payment request");

                if (request != null)
                {
                    await _paymentStatusProducer.PublishFailureStatusAsync(request, "Deserialization error", stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing check image");

                if (request != null)
                {
                    await _paymentStatusProducer.PublishFailureStatusAsync(request, ex.Message, stoppingToken);
                }
            }
        }
    }
}