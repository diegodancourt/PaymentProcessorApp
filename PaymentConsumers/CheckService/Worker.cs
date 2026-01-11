using CheckService.Infrastructure.Consumers;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace CheckService;

public class Worker(
    ILogger<Worker> logger,
    IOptions<KafkaSettings> kafkaSettings,
    PaymentRequestConsumer paymentRequestConsumer) : BackgroundService
{
    private readonly KafkaSettings _kafkaSettings = kafkaSettings.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _kafkaSettings.BootstrapServers,
            GroupId = _kafkaSettings.GroupId,
            AutoOffsetReset = _kafkaSettings.AutoOffsetReset
                ? AutoOffsetReset.Earliest
                : AutoOffsetReset.Latest,
            EnableAutoCommit = _kafkaSettings.EnableAutoCommit
        };

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _kafkaSettings.BootstrapServers,
            Acks = Acks.All,
            MessageTimeoutMs = _kafkaSettings.ProducerTimeoutMs
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        using var producer = new ProducerBuilder<Null, string>(producerConfig).Build();

        consumer.Subscribe(_kafkaSettings.ConsumerTopic);
        logger.LogInformation("Subscribed to topic: {Topic}", _kafkaSettings.ConsumerTopic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await paymentRequestConsumer.ConsumeMessageAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Consumer shutdown requested");
        }
        finally
        {
            consumer.Close();
            producer.Flush(TimeSpan.FromSeconds(10));
        }
    }
}