using CardService;
using CardService.Infrastructure.Consumers;
using CardService.Infrastructure.Producers;
using CheckService;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddSerilog();

builder.Services.Configure<KafkaSettings>(
    builder.Configuration.GetSection(KafkaSettings.SectionName));

builder.Services.AddSingleton<PaymentRequestConsumer>();
builder.Services.AddSingleton<PaymentStatusProducer>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

try
{
    Log.Information("Starting CardService Host...");
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "CardService terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}