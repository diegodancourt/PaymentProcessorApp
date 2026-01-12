using Amazon.DynamoDBv2;
using Amazon.Runtime;
using LedgerService;
using LedgerService.Services;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddSerilog();

builder.Services.Configure<KafkaSettings>(
    builder.Configuration.GetSection(KafkaSettings.SectionName));

var dynamoDbSettings = builder.Configuration
    .GetSection(DynamoDbSettings.SectionName)
    .Get<DynamoDbSettings>();

builder.Services.Configure<DynamoDbSettings>(
    builder.Configuration.GetSection(DynamoDbSettings.SectionName));

// Register DynamoDB client
builder.Services.AddSingleton<IAmazonDynamoDB>(sp =>
{
    var config = new AmazonDynamoDBConfig
    {
        ServiceURL = dynamoDbSettings?.ServiceUrl
    };

    if (!string.IsNullOrEmpty(dynamoDbSettings?.Region))
    {
        config.RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(dynamoDbSettings.Region);
    }

    // Use credentials if provided, otherwise use default credentials
    if (!string.IsNullOrEmpty(dynamoDbSettings?.AccessKey) && !string.IsNullOrEmpty(dynamoDbSettings?.SecretKey))
    {
        var credentials = new BasicAWSCredentials(dynamoDbSettings.AccessKey, dynamoDbSettings.SecretKey);
        return new AmazonDynamoDBClient(credentials, config);
    }

    return new AmazonDynamoDBClient(config);
});

builder.Services.AddSingleton<ILedgerRepository, DynamoDbLedgerRepository>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

try
{
    Log.Information("Starting LedgerService");
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "LedgerService terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}