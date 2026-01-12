namespace NotificationService;

public class CustomerApiSettings
{
    public const string SectionName = "CustomerApi";

    public required string BaseUrl { get; init; }
    public int TimeoutSeconds { get; init; } = 30;
}