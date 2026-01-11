namespace NotificationService;

public class EmailSettings
{
    public const string SectionName = "Email";

    public required string SmtpServer { get; init; }
    public int SmtpPort { get; init; } = 587;
    public required string FromEmail { get; init; }
    public required string FromName { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }
    public bool EnableSsl { get; init; } = true;
}