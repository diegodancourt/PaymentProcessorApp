using CheckReader.Services;
using CheckService;
using CheckService.Infrastructure.Consumers;
using CheckService.Infrastructure.Producers;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddSerilog();

builder.Services.Configure<KafkaSettings>(
    builder.Configuration.GetSection(KafkaSettings.SectionName));

builder.Services.AddSingleton<ICheckReader, CheckReader.Services.CheckReader>();
builder.Services.AddSingleton<PaymentRequestConsumer>();
builder.Services.AddSingleton<PaymentStatusProducer>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

try
{
    Log.Information("Starting CheckService");
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "CheckService terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}