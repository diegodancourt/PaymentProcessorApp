namespace NotificationService.Tests;

public class EmailSettingsTests
{
    [Fact]
    public void SectionName_ShouldBeEmail()
    {
        Assert.Equal("Email", EmailSettings.SectionName);
    }

    [Fact]
    public void EmailSettings_ShouldInitializeWithRequiredProperties()
    {
        var settings = new EmailSettings
        {
            SmtpServer = "smtp.test.com",
            FromEmail = "test@example.com",
            FromName = "Test Sender"
        };

        Assert.Equal("smtp.test.com", settings.SmtpServer);
        Assert.Equal("test@example.com", settings.FromEmail);
        Assert.Equal("Test Sender", settings.FromName);
    }

    [Fact]
    public void SmtpPort_ShouldDefaultTo587()
    {
        var settings = new EmailSettings
        {
            SmtpServer = "smtp.test.com",
            FromEmail = "test@example.com",
            FromName = "Test Sender"
        };

        Assert.Equal(587, settings.SmtpPort);
    }

    [Fact]
    public void EnableSsl_ShouldDefaultToTrue()
    {
        var settings = new EmailSettings
        {
            SmtpServer = "smtp.test.com",
            FromEmail = "test@example.com",
            FromName = "Test Sender"
        };

        Assert.True(settings.EnableSsl);
    }

    [Fact]
    public void EmailSettings_ShouldAllowCustomSmtpPort()
    {
        var settings = new EmailSettings
        {
            SmtpServer = "smtp.test.com",
            FromEmail = "test@example.com",
            FromName = "Test Sender",
            SmtpPort = 465
        };

        Assert.Equal(465, settings.SmtpPort);
    }

    [Fact]
    public void EmailSettings_ShouldAllowOptionalCredentials()
    {
        var settings = new EmailSettings
        {
            SmtpServer = "smtp.test.com",
            FromEmail = "test@example.com",
            FromName = "Test Sender",
            Username = "user@example.com",
            Password = "password123"
        };

        Assert.Equal("user@example.com", settings.Username);
        Assert.Equal("password123", settings.Password);
    }

    [Fact]
    public void EmailSettings_ShouldAllowDisablingSSL()
    {
        var settings = new EmailSettings
        {
            SmtpServer = "smtp.test.com",
            FromEmail = "test@example.com",
            FromName = "Test Sender",
            EnableSsl = false
        };

        Assert.False(settings.EnableSsl);
    }
}