using NotificationService;
using NotificationService.Services;
using Refit;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddSerilog();

builder.Services.Configure<KafkaSettings>(
    builder.Configuration.GetSection(KafkaSettings.SectionName));

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection(EmailSettings.SectionName));

var customerApiSettings = builder.Configuration
    .GetSection(CustomerApiSettings.SectionName)
    .Get<CustomerApiSettings>();

builder.Services.Configure<CustomerApiSettings>(
    builder.Configuration.GetSection(CustomerApiSettings.SectionName));

// Register Refit client for CustomerApi
builder.Services.AddRefitClient<ICustomerApiRefitClient>()
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri(customerApiSettings?.BaseUrl ?? "http://localhost:5000");
        c.Timeout = TimeSpan.FromSeconds(customerApiSettings?.TimeoutSeconds ?? 30);
    });

builder.Services.AddSingleton<ICustomerApiClient, CustomerApiClient>();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

try
{
    Log.Information("Starting NotificationService");
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "NotificationService terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}