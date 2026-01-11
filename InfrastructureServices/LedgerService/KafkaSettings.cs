namespace LedgerService;

public class KafkaSettings
{
    public const string SectionName = "Kafka";

    public required string BootstrapServers { get; init; }
    public required string GroupId { get; init; }
    public required string Topic { get; init; }
    public bool AutoOffsetReset { get; init; } = true;
    public bool EnableAutoCommit { get; init; } = false;
}