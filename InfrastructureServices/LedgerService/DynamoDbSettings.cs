namespace LedgerService;

public class DynamoDbSettings
{
    public const string SectionName = "DynamoDb";

    public required string TableName { get; init; }
    public required string ServiceUrl { get; init; }
    public string? AccessKey { get; init; }
    public string? SecretKey { get; init; }
    public string Region { get; init; } = "us-east-1";
}