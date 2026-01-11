namespace NotificationService.Tests;

public class KafkaSettingsTests
{
    [Fact]
    public void SectionName_ShouldBeKafka()
    {
        Assert.Equal("Kafka", KafkaSettings.SectionName);
    }

    [Fact]
    public void KafkaSettings_ShouldInitializeWithRequiredProperties()
    {
        var settings = new KafkaSettings
        {
            BootstrapServers = "localhost:9092",
            GroupId = "test-group",
            Topic = "test-topic"
        };

        Assert.Equal("localhost:9092", settings.BootstrapServers);
        Assert.Equal("test-group", settings.GroupId);
        Assert.Equal("test-topic", settings.Topic);
    }

    [Fact]
    public void AutoOffsetReset_ShouldDefaultToTrue()
    {
        var settings = new KafkaSettings
        {
            BootstrapServers = "localhost:9092",
            GroupId = "test-group",
            Topic = "test-topic"
        };

        Assert.True(settings.AutoOffsetReset);
    }

    [Fact]
    public void EnableAutoCommit_ShouldDefaultToFalse()
    {
        var settings = new KafkaSettings
        {
            BootstrapServers = "localhost:9092",
            GroupId = "test-group",
            Topic = "test-topic"
        };

        Assert.False(settings.EnableAutoCommit);
    }

    [Fact]
    public void KafkaSettings_ShouldAllowCustomAutoOffsetReset()
    {
        var settings = new KafkaSettings
        {
            BootstrapServers = "localhost:9092",
            GroupId = "test-group",
            Topic = "test-topic",
            AutoOffsetReset = false
        };

        Assert.False(settings.AutoOffsetReset);
    }

    [Fact]
    public void KafkaSettings_ShouldAllowCustomEnableAutoCommit()
    {
        var settings = new KafkaSettings
        {
            BootstrapServers = "localhost:9092",
            GroupId = "test-group",
            Topic = "test-topic",
            EnableAutoCommit = true
        };

        Assert.True(settings.EnableAutoCommit);
    }
}