namespace PaymentOrchestratorService.Configuration;

public class KafkaSettings
{
    public const string SectionName = "Kafka";

    public required string BootstrapServers { get; init; }
    public required string CheckPaymentTopic { get; init; }
    public required string CardPaymentTopic { get; init; }
    public int ProducerTimeoutMs { get; init; } = 5000;
}