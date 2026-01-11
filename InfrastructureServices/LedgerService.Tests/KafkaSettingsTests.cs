namespace LedgerService.Tests;

public class KafkaSettingsTests
{
    [Fact]
    public void SectionName_ShouldBe_Kafka()
    {
        // Arrange & Act
        var sectionName = KafkaSettings.SectionName;

        // Assert
        sectionName.Should().Be("Kafka");
    }

    [Fact]
    public void BootstrapServers_ShouldBeSettable()
    {
        // Arrange & Act
        var settings = new KafkaSettings
        {
            BootstrapServers = "localhost:9092",
            GroupId = "test-group",
            Topic = "test-topic",
            AutoOffsetReset = true,
            EnableAutoCommit = false
        };

        // Assert
        settings.BootstrapServers.Should().Be("localhost:9092");
    }

    [Fact]
    public void GroupId_ShouldBeSettable()
    {
        // Arrange & Act
        var settings = new KafkaSettings
        {
            BootstrapServers = "localhost:9092",
            GroupId = "test-group",
            Topic = "test-topic",
            AutoOffsetReset = true,
            EnableAutoCommit = false
        };

        // Assert
        settings.GroupId.Should().Be("test-group");
    }

    [Fact]
    public void Topic_ShouldBeSettable()
    {
        // Arrange & Act
        var settings = new KafkaSettings
        {
            BootstrapServers = "localhost:9092",
            GroupId = "test-group",
            Topic = "test-topic",
            AutoOffsetReset = true,
            EnableAutoCommit = false
        };

        // Assert
        settings.Topic.Should().Be("test-topic");
    }

    [Fact]
    public void AutoOffsetReset_ShouldBeSettable()
    {
        // Arrange & Act
        var settings = new KafkaSettings
        {
            BootstrapServers = "localhost:9092",
            GroupId = "test-group",
            Topic = "test-topic",
            AutoOffsetReset = true,
            EnableAutoCommit = false
        };

        // Assert
        settings.AutoOffsetReset.Should().BeTrue();
    }

    [Fact]
    public void EnableAutoCommit_ShouldBeSettable()
    {
        // Arrange & Act
        var settings = new KafkaSettings
        {
            BootstrapServers = "localhost:9092",
            GroupId = "test-group",
            Topic = "test-topic",
            AutoOffsetReset = true,
            EnableAutoCommit = false
        };

        // Assert
        settings.EnableAutoCommit.Should().BeFalse();
    }
}
