using System.Text.Json;
using Confluent.Kafka;
using LedgerService.Domain;
using LedgerService.Services;
using Microsoft.Extensions.Options;

namespace LedgerService;

public class Worker(
    ILogger<Worker> logger,
    IOptions<KafkaSettings> kafkaSettings,
    ILedgerRepository ledgerRepository) : BackgroundService
{
    private readonly KafkaSettings _kafkaSettings = kafkaSettings.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _kafkaSettings.BootstrapServers,
            GroupId = _kafkaSettings.GroupId,
            AutoOffsetReset = _kafkaSettings.AutoOffsetReset
                ? Confluent.Kafka.AutoOffsetReset.Earliest
                : Confluent.Kafka.AutoOffsetReset.Latest,
            EnableAutoCommit = _kafkaSettings.EnableAutoCommit
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();

        consumer.Subscribe(_kafkaSettings.Topic);
        logger.LogInformation("Subscribed to topic: {Topic}", _kafkaSettings.Topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ConsumeMessageAsync(consumer, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Consumer shutdown requested");
        }
        finally
        {
            consumer.Close();
        }
    }

    private async Task ConsumeMessageAsync(IConsumer<Ignore, string> consumer, CancellationToken stoppingToken)
    {
        try
        {
            var consumeResult = consumer.Consume(stoppingToken);

            if (consumeResult?.Message?.Value == null)
            {
                return;
            }

            var messageJson = consumeResult.Message.Value;
            logger.LogInformation(
                "Received payment status message. Partition: {Partition}, Offset: {Offset}",
                consumeResult.Partition.Value,
                consumeResult.Offset.Value);

            var paymentStatus = JsonSerializer.Deserialize<PaymentStatus>(messageJson);

            if (paymentStatus == null)
            {
                logger.LogWarning("Failed to deserialize payment status message");
                return;
            }

            logger.LogInformation(
                "Processing payment status - PaymentId: {PaymentId}, Status: {Status}, CustomerId: {CustomerId}",
                paymentStatus.PaymentId,
                paymentStatus.Status,
                paymentStatus.CustomerId);

            // Save to DynamoDB
            await ledgerRepository.SavePaymentStatusAsync(paymentStatus);

            logger.LogInformation(
                "Payment status saved to ledger - PaymentId: {PaymentId}",
                paymentStatus.PaymentId);

            consumer.Commit(consumeResult);
        }
        catch (ConsumeException ex)
        {
            logger.LogError(ex, "Error consuming message: {Reason}", ex.Error.Reason);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Error deserializing payment status message");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing payment status for ledger");
        }
    }
}