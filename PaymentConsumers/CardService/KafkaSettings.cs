namespace CheckService;

public class KafkaSettings
{
    public const string SectionName = "Kafka";

    public required string BootstrapServers { get; init; }
    public required string GroupId { get; init; }
    public required string ConsumerTopic { get; init; }
    public required string ProducerTopic { get; init; }
    public bool AutoOffsetReset { get; init; } = true;
    public bool EnableAutoCommit { get; init; } = false;
    public int ProducerTimeoutMs { get; init; } = 5000;
}