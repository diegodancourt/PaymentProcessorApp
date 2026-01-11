namespace NotificationService.Tests;

public class CustomerApiSettingsTests
{
    [Fact]
    public void SectionName_ShouldBeCustomerApi()
    {
        Assert.Equal("CustomerApi", CustomerApiSettings.SectionName);
    }

    [Fact]
    public void CustomerApiSettings_ShouldInitializeWithRequiredProperties()
    {
        var settings = new CustomerApiSettings
        {
            BaseUrl = "http://localhost:5000"
        };

        Assert.Equal("http://localhost:5000", settings.BaseUrl);
    }

    [Fact]
    public void TimeoutSeconds_ShouldDefaultTo30()
    {
        var settings = new CustomerApiSettings
        {
            BaseUrl = "http://localhost:5000"
        };

        Assert.Equal(30, settings.TimeoutSeconds);
    }

    [Fact]
    public void CustomerApiSettings_ShouldAllowCustomTimeout()
    {
        var settings = new CustomerApiSettings
        {
            BaseUrl = "http://localhost:5000",
            TimeoutSeconds = 60
        };

        Assert.Equal(60, settings.TimeoutSeconds);
    }
}