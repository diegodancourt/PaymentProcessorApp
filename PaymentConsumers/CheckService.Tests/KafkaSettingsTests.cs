namespace CheckService.Tests;

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
            ConsumerTopic = "test-consumer-topic",
            ProducerTopic = "test-producer-topic"
        };

        Assert.Equal("localhost:9092", settings.BootstrapServers);
        Assert.Equal("test-group", settings.GroupId);
        Assert.Equal("test-consumer-topic", settings.ConsumerTopic);
        Assert.Equal("test-producer-topic", settings.ProducerTopic);
    }

    [Fact]
    public void AutoOffsetReset_ShouldDefaultToTrue()
    {
        var settings = new KafkaSettings
        {
            BootstrapServers = "localhost:9092",
            GroupId = "test-group",
            ConsumerTopic = "test-consumer-topic",
            ProducerTopic = "test-producer-topic"
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
            ConsumerTopic = "test-consumer-topic",
            ProducerTopic = "test-producer-topic"
        };

        Assert.False(settings.EnableAutoCommit);
    }

    [Fact]
    public void ProducerTimeoutMs_ShouldDefaultTo5000()
    {
        var settings = new KafkaSettings
        {
            BootstrapServers = "localhost:9092",
            GroupId = "test-group",
            ConsumerTopic = "test-consumer-topic",
            ProducerTopic = "test-producer-topic"
        };

        Assert.Equal(5000, settings.ProducerTimeoutMs);
    }

    [Fact]
    public void KafkaSettings_ShouldAllowCustomAutoOffsetReset()
    {
        var settings = new KafkaSettings
        {
            BootstrapServers = "localhost:9092",
            GroupId = "test-group",
            ConsumerTopic = "test-consumer-topic",
            ProducerTopic = "test-producer-topic",
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
            ConsumerTopic = "test-consumer-topic",
            ProducerTopic = "test-producer-topic",
            EnableAutoCommit = true
        };

        Assert.True(settings.EnableAutoCommit);
    }

    [Fact]
    public void KafkaSettings_ShouldAllowCustomProducerTimeoutMs()
    {
        var settings = new KafkaSettings
        {
            BootstrapServers = "localhost:9092",
            GroupId = "test-group",
            ConsumerTopic = "test-consumer-topic",
            ProducerTopic = "test-producer-topic",
            ProducerTimeoutMs = 10000
        };

        Assert.Equal(10000, settings.ProducerTimeoutMs);
    }
}