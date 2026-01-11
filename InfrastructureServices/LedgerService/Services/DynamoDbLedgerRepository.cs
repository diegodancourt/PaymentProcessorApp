using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using LedgerService.Domain;
using Microsoft.Extensions.Options;

namespace LedgerService.Services;

public class DynamoDbLedgerRepository : ILedgerRepository
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly ILogger<DynamoDbLedgerRepository> _logger;
    private readonly DynamoDbSettings _settings;

    public DynamoDbLedgerRepository(
        IAmazonDynamoDB dynamoDbClient,
        ILogger<DynamoDbLedgerRepository> logger,
        IOptions<DynamoDbSettings> settings)
    {
        _dynamoDbClient = dynamoDbClient;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task SavePaymentStatusAsync(PaymentStatus paymentStatus)
    {
        try
        {
            _logger.LogInformation(
                "Saving payment status to DynamoDB - PaymentId: {PaymentId}, Status: {Status}",
                paymentStatus.PaymentId,
                paymentStatus.Status);

            var item = new Dictionary<string, AttributeValue>
            {
                ["PaymentId"] = new AttributeValue { S = paymentStatus.PaymentId },
                ["CustomerId"] = new AttributeValue { S = paymentStatus.CustomerId.ToString() },
                ["Amount"] = new AttributeValue { N = paymentStatus.Amount.ToString("F2") },
                ["Status"] = new AttributeValue { S = paymentStatus.Status },
                ["Timestamp"] = new AttributeValue { S = paymentStatus.Timestamp.ToString("O") },
                ["CreatedAt"] = new AttributeValue { S = DateTime.UtcNow.ToString("O") }
            };

            if (paymentStatus.PaymentMethod != null)
            {
                item["PaymentMethod"] = new AttributeValue { S = paymentStatus.PaymentMethod };
            }

            if (paymentStatus.ErrorMessage != null)
            {
                item["ErrorMessage"] = new AttributeValue { S = paymentStatus.ErrorMessage };
            }

            var request = new PutItemRequest
            {
                TableName = _settings.TableName,
                Item = item
            };

            await _dynamoDbClient.PutItemAsync(request);

            _logger.LogInformation(
                "Successfully saved payment status to DynamoDB - PaymentId: {PaymentId}",
                paymentStatus.PaymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to save payment status to DynamoDB - PaymentId: {PaymentId}",
                paymentStatus.PaymentId);
            throw;
        }
    }

    public async Task<PaymentStatus?> GetPaymentStatusAsync(string paymentId)
    {
        try
        {
            _logger.LogInformation("Retrieving payment status from DynamoDB - PaymentId: {PaymentId}", paymentId);

            var request = new GetItemRequest
            {
                TableName = _settings.TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    ["PaymentId"] = new AttributeValue { S = paymentId }
                }
            };

            var response = await _dynamoDbClient.GetItemAsync(request);

            if (response.Item == null || response.Item.Count == 0)
            {
                _logger.LogWarning("Payment status not found in DynamoDB - PaymentId: {PaymentId}", paymentId);
                return null;
            }

            var paymentStatus = new PaymentStatus
            {
                PaymentId = response.Item["PaymentId"].S,
                CustomerId = Guid.Parse(response.Item["CustomerId"].S),
                Amount = decimal.Parse(response.Item["Amount"].N),
                Status = response.Item["Status"].S,
                Timestamp = DateTime.Parse(response.Item["Timestamp"].S),
                PaymentMethod = response.Item.ContainsKey("PaymentMethod") ? response.Item["PaymentMethod"].S : null,
                ErrorMessage = response.Item.ContainsKey("ErrorMessage") ? response.Item["ErrorMessage"].S : null
            };

            _logger.LogInformation("Successfully retrieved payment status from DynamoDB - PaymentId: {PaymentId}", paymentId);
            return paymentStatus;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve payment status from DynamoDB - PaymentId: {PaymentId}", paymentId);
            return null;
        }
    }
}