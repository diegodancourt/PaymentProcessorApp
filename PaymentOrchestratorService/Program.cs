using PaymentOrchestratorService.Configuration;
using PaymentOrchestratorService.Models;
using PaymentOrchestratorService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddOpenApi();

// Configure Kafka settings
builder.Services.Configure<KafkaSettings>(
    builder.Configuration.GetSection(KafkaSettings.SectionName));

// Register payment producer
builder.Services.AddSingleton<IPaymentProducer, PaymentProducer>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// POST: Submit check payment
app.MapPost("/api/payments/check", async (
    CheckPaymentRequest request,
    IPaymentProducer paymentProducer,
    ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation(
            "Received check payment request from CustomerId: {CustomerId}, ImageSize: {ImageSize} bytes",
            request.CustomerId,
            request.ImageData.Length);

        var paymentId = await paymentProducer.PublishCheckPaymentAsync(
            request.CustomerId,
            request.ImageData);

        return Results.Accepted(
            $"/api/payments/{paymentId}",
            new PaymentResponse
            {
                PaymentId = paymentId,
                Status = "Pending",
                Message = "Check payment request has been accepted for processing"
            });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error processing check payment request");
        return Results.Problem(
            detail: "Failed to process check payment request",
            statusCode: StatusCodes.Status500InternalServerError);
    }
})
.WithName("SubmitCheckPayment")
.WithSummary("Submit a check payment for processing")
.WithDescription("Uploads a check image and initiates payment processing via OCR");

// POST: Submit card payment
app.MapPost("/api/payments/card", async (
    CardPaymentRequest request,
    IPaymentProducer paymentProducer,
    ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation(
            "Received card payment request from CustomerId: {CustomerId}, Amount: {Amount}",
            request.CustomerId,
            request.Amount);

        // Basic validation
        if (request.Amount <= 0)
        {
            return Results.BadRequest(new { message = "Amount must be greater than zero" });
        }

        if (string.IsNullOrWhiteSpace(request.CardNumber))
        {
            return Results.BadRequest(new { message = "Card number is required" });
        }

        var paymentId = await paymentProducer.PublishCardPaymentAsync(
            request.CustomerId,
            request.CardNumber,
            request.ExpiryDate,
            request.Cvv,
            request.Amount);

        return Results.Accepted(
            $"/api/payments/{paymentId}",
            new PaymentResponse
            {
                PaymentId = paymentId,
                Status = "Pending",
                Message = "Card payment request has been accepted for processing"
            });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error processing card payment request");
        return Results.Problem(
            detail: "Failed to process card payment request",
            statusCode: StatusCodes.Status500InternalServerError);
    }
})
.WithName("SubmitCardPayment")
.WithSummary("Submit a card payment for processing")
.WithDescription("Processes a credit/debit card payment");

// GET: Health check
app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy",
    Service = "PaymentOrchestratorService",
    Timestamp = DateTime.UtcNow
}))
.WithName("HealthCheck")
.WithSummary("Service health check");

app.Run();