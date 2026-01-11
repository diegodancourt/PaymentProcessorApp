namespace LedgerService.Tests;

public class DynamoDbSettingsTests
{
    [Fact]
    public void SectionName_ShouldBe_DynamoDb()
    {
        // Arrange & Act
        var sectionName = DynamoDbSettings.SectionName;

        // Assert
        sectionName.Should().Be("DynamoDb");
    }

    [Fact]
    public void TableName_ShouldBeSettable()
    {
        // Arrange & Act
        var settings = new DynamoDbSettings
        {
            TableName = "PaymentLedger",
            ServiceUrl = "http://localhost:8000",
            Region = "us-east-1"
        };

        // Assert
        settings.TableName.Should().Be("PaymentLedger");
    }

    [Fact]
    public void ServiceUrl_ShouldBeSettable()
    {
        // Arrange & Act
        var settings = new DynamoDbSettings
        {
            TableName = "PaymentLedger",
            ServiceUrl = "http://localhost:8000",
            Region = "us-east-1"
        };

        // Assert
        settings.ServiceUrl.Should().Be("http://localhost:8000");
    }

    [Fact]
    public void Region_ShouldBeSettable()
    {
        // Arrange & Act
        var settings = new DynamoDbSettings
        {
            TableName = "PaymentLedger",
            ServiceUrl = "http://localhost:8000",
            Region = "us-west-2"
        };

        // Assert
        settings.Region.Should().Be("us-west-2");
    }

    [Fact]
    public void Region_ShouldHaveDefaultValue()
    {
        // Arrange & Act
        var settings = new DynamoDbSettings
        {
            TableName = "PaymentLedger",
            ServiceUrl = "http://localhost:8000"
        };

        // Assert
        settings.Region.Should().Be("us-east-1");
    }

    [Fact]
    public void AccessKey_ShouldBeOptional()
    {
        // Arrange & Act
        var settings = new DynamoDbSettings
        {
            TableName = "PaymentLedger",
            ServiceUrl = "http://localhost:8000",
            AccessKey = "test-key"
        };

        // Assert
        settings.AccessKey.Should().Be("test-key");
    }

    [Fact]
    public void SecretKey_ShouldBeOptional()
    {
        // Arrange & Act
        var settings = new DynamoDbSettings
        {
            TableName = "PaymentLedger",
            ServiceUrl = "http://localhost:8000",
            SecretKey = "test-secret"
        };

        // Assert
        settings.SecretKey.Should().Be("test-secret");
    }
}
