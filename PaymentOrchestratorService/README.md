# Payment Orchestrator Service

The entry point REST API for the Payment Processor Application. This service receives payment requests from clients and publishes them to Kafka topics for asynchronous processing.

## Overview

The Payment Orchestrator Service acts as the gateway for all payment operations. It accepts payment requests via HTTP, validates them, assigns unique payment IDs, and publishes them to appropriate Kafka topics based on the payment method.

## Architecture

```
┌─────────────────┐
│   Client App    │
└────────┬────────┘
         │ HTTP POST
         ▼
┌─────────────────────────┐
│ PaymentOrchestrator API │
│  - Validate Request     │
│  - Generate Payment ID  │
│  - Publish to Kafka     │
└────────┬────────────────┘
         │
         ▼
┌─────────────────────────┐
│    Apache Kafka         │
│  - check-payment-       │
│    requests             │
│  - card-payment-        │
│    requests             │
└─────────────────────────┘
```

## API Endpoints

### Submit Check Payment
**POST** `/api/payments/check`

Submits a check payment for processing via OCR.

**Request Body:**
```json
{
  "customerId": "11111111-1111-1111-1111-111111111111",
  "imageData": "<base64-encoded-check-image>"
}
```

**Response:** `202 Accepted`
```json
{
  "paymentId": "payment-abc123def456",
  "status": "Pending",
  "message": "Check payment request has been accepted for processing",
  "timestamp": "2026-01-11T10:30:00Z"
}
```

**Example:**
```bash
curl -X POST http://localhost:5000/api/payments/check \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "11111111-1111-1111-1111-111111111111",
    "imageData": "/9j/4AAQSkZJRgABAQEAYABgAAD..."
  }'
```

### Submit Card Payment
**POST** `/api/payments/card`

Submits a credit/debit card payment for processing.

**Request Body:**
```json
{
  "customerId": "11111111-1111-1111-1111-111111111111",
  "cardNumber": "4111111111111111",
  "expiryDate": "12/25",
  "cvv": "123",
  "amount": 150.75
}
```

**Validation Rules:**
- `amount` must be greater than 0
- `cardNumber` is required
- `customerId` must be a valid GUID

**Response:** `202 Accepted`
```json
{
  "paymentId": "payment-xyz789ghi012",
  "status": "Pending",
  "message": "Card payment request has been accepted for processing",
  "timestamp": "2026-01-11T10:30:00Z"
}
```

**Error Response:** `400 Bad Request`
```json
{
  "message": "Amount must be greater than zero"
}
```

**Example:**
```bash
curl -X POST http://localhost:5000/api/payments/card \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "11111111-1111-1111-1111-111111111111",
    "cardNumber": "4111111111111111",
    "expiryDate": "12/25",
    "cvv": "123",
    "amount": 150.75
  }'
```

### Health Check
**GET** `/health`

Returns the health status of the service.

**Response:** `200 OK`
```json
{
  "status": "Healthy",
  "service": "PaymentOrchestratorService",
  "timestamp": "2026-01-11T10:30:00Z"
}
```

## Configuration

### appsettings.json

```json
{
  "Kafka": {
    "BootstrapServers": "localhost:9092",
    "CheckPaymentTopic": "check-payment-requests",
    "CardPaymentTopic": "card-payment-requests",
    "ProducerTimeoutMs": 5000
  }
}
```

### Environment Variables

Configuration can be overridden using environment variables:

```bash
export Kafka__BootstrapServers="kafka:9092"
export Kafka__CheckPaymentTopic="check-payment-requests"
export Kafka__CardPaymentTopic="card-payment-requests"
```

## Running the Service

### Local Development

```bash
# Start Kafka (required)
docker run -d --name kafka -p 9092:9092 \
  -e KAFKA_ZOOKEEPER_CONNECT=localhost:2181 \
  confluentinc/cp-kafka:latest

# Run the service
dotnet run --project PaymentOrchestratorService/PaymentOrchestratorService.csproj
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- OpenAPI: `http://localhost:5000/openapi/v1.json`

### Docker

```bash
# Build image
docker build -t payment-orchestrator \
  -f PaymentOrchestratorService/Dockerfile .

# Run container
docker run -d -p 5000:8080 \
  -e Kafka__BootstrapServers=kafka:9092 \
  payment-orchestrator
```

## Message Flow

### Check Payment Request

1. Client submits check image to `/api/payments/check`
2. Service generates unique payment ID
3. Service publishes message to `check-payment-requests` topic:
   ```json
   {
     "paymentId": "payment-abc123",
     "customerId": "11111111-1111-1111-1111-111111111111",
     "imageData": "<byte-array>"
   }
   ```
4. CheckService consumes message and processes check via OCR
5. CheckService publishes result to `payment-status` topic

### Card Payment Request

1. Client submits card details to `/api/payments/card`
2. Service validates request (amount > 0, card number present)
3. Service generates unique payment ID
4. Service publishes message to `card-payment-requests` topic:
   ```json
   {
     "paymentId": "payment-xyz789",
     "customerId": "11111111-1111-1111-1111-111111111111",
     "cardNumber": "4111111111111111",
     "expiryDate": "12/25",
     "cvv": "123",
     "amount": 150.75
   }
   ```
5. CardService consumes message and processes payment
6. CardService publishes result to `payment-status` topic

## Project Structure

```
PaymentOrchestratorService/
├── Configuration/
│   └── KafkaSettings.cs          # Kafka configuration settings
├── Models/
│   ├── CheckPaymentRequest.cs    # Check payment request DTO
│   ├── CardPaymentRequest.cs     # Card payment request DTO
│   └── PaymentResponse.cs        # Payment response DTO
├── Services/
│   ├── IPaymentProducer.cs       # Payment producer interface
│   └── PaymentProducer.cs        # Kafka producer implementation
├── Properties/
│   └── launchSettings.json       # Launch configuration
├── Program.cs                     # API endpoints and DI setup
├── appsettings.json              # Configuration
├── Dockerfile                     # Container image definition
└── README.md                      # This file
```

## Dependencies

- **ASP.NET Core 10.0** - Web framework
- **Confluent.Kafka 2.6.1** - Kafka client library
- **Microsoft.AspNetCore.OpenApi 10.0.0** - OpenAPI documentation

## Error Handling

### 400 Bad Request
- Invalid request data (missing fields, invalid formats)
- Business validation failures (amount <= 0)

### 500 Internal Server Error
- Kafka connection failures
- Message publishing failures
- Unexpected errors

All errors are logged with full stack traces for debugging.

## Logging

The service logs the following events:
- Received payment requests (with customer ID and metadata)
- Published Kafka messages (with topic, partition, offset)
- Errors during processing

### Log Levels

- **Debug**: Detailed diagnostic information (Development only)
- **Information**: Request/response flow, successful operations
- **Warning**: Validation failures, retryable errors
- **Error**: Message publishing failures, unhandled exceptions

## OpenAPI Documentation

OpenAPI documentation is available in development mode:

- **Spec**: `http://localhost:5000/openapi/v1.json`

Import this spec into tools like Swagger UI, Postman, or Insomnia for interactive API testing.

## Testing

### Manual Testing with curl

```bash
# Health check
curl http://localhost:5000/health

# Submit check payment
curl -X POST http://localhost:5000/api/payments/check \
  -H "Content-Type: application/json" \
  -d @check-payment.json

# Submit card payment
curl -X POST http://localhost:5000/api/payments/card \
  -H "Content-Type: application/json" \
  -d @card-payment.json
```

### Verify Kafka Messages

```bash
# Check payment requests
docker exec kafka kafka-console-consumer \
  --bootstrap-server localhost:9092 \
  --topic check-payment-requests \
  --from-beginning

# Card payment requests
docker exec kafka kafka-console-consumer \
  --bootstrap-server localhost:9092 \
  --topic card-payment-requests \
  --from-beginning
```

## Monitoring

### Health Check Endpoint

Use `/health` for:
- Container health checks
- Kubernetes liveness/readiness probes
- Load balancer health monitoring

### Metrics (Future Enhancement)

Planned metrics to track:
- Payment request rate (requests/second)
- Kafka publish success/failure rate
- Request latency (p50, p95, p99)
- Error rate by payment method

## Security Considerations

⚠️ **Current Implementation**

This is a development implementation. For production:

- [ ] Add authentication/authorization (JWT, OAuth2)
- [ ] Implement rate limiting
- [ ] Add request validation middleware
- [ ] Encrypt sensitive data (card numbers, CVV)
- [ ] Use HTTPS only
- [ ] Implement PCI DSS compliance for card payments
- [ ] Add API versioning
- [ ] Implement idempotency keys

## Next Steps

1. Implement authentication middleware
2. Add comprehensive input validation
3. Implement payment status query endpoint (`GET /api/payments/{id}`)
4. Add webhook support for payment status callbacks
5. Implement retry logic for Kafka publishing failures
6. Add metrics and distributed tracing
7. Create integration tests

## Related Services

- **CheckService**: Processes check payments via OCR
- **CardService**: Processes card payments via payment gateway
- **LedgerService**: Records payment transactions
- **NotificationService**: Sends payment notifications to customers

For complete system architecture, see the [main README](../README.md).