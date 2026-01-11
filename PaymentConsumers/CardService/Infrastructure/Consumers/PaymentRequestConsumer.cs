using System.Text.Json;
using CardService.Domain;
using CardService.Infrastructure.Producers;
using Confluent.Kafka;

namespace CardService.Infrastructure.Consumers
{
    public class PaymentRequestConsumer
    {
        private readonly ILogger<PaymentRequestConsumer> _logger;
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly PaymentStatusProducer _paymentStatusProducer;

        public PaymentRequestConsumer(ILogger<PaymentRequestConsumer> logger, IConsumer<Ignore, string> consumer,
            PaymentStatusProducer paymentStatusProducer)
        {
            _logger = logger;
            _consumer = consumer;
            _paymentStatusProducer = paymentStatusProducer;
        }

        public async Task ConsumeMessageAsync(CancellationToken stoppingToken)
        {
            CardPaymentRequest? request = null;
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
                    "Received card payment request. Partition: {Partition}, Offset: {Offset}",
                    consumeResult.Partition.Value,
                    consumeResult.Offset.Value);

                request = JsonSerializer.Deserialize<CardPaymentRequest>(messageJson);
                if (request == null)
                {
                    _logger.LogError("Deserialized CardPaymentRequest is null");
                    _consumer.Commit(consumeResult);
                    return;
                }

                //TODO: Send to Card Processing API --Here we will use the CardToken

                // Create and publish PaymentStatus
                var paymentStatus = new PaymentStatus
                {
                    PaymentId = request.PaymentId,
                    CustomerId = request.CustomerId,
                    Amount = request.Amount,
                    Status = "Success",
                    Timestamp = DateTime.UtcNow,
                    PaymentMethod = "Card"
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
                    await _paymentStatusProducer.PublishFailureStatusAsync(request, "Deserialization error",
                        stoppingToken);
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