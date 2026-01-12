# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
# Build the entire solution
dotnet build PaymentProcessorApp.sln

# Build individual services
dotnet build PaymentOrchestratorService/PaymentOrchestratorService.csproj
dotnet build PaymentConsumers/CheckService/CheckService.csproj
dotnet build PaymentConsumers/CardService/CardService.csproj
dotnet build InfrastructureServices/LedgerService/LedgerService.csproj
dotnet build InfrastructureServices/NotificationService/NotificationService.csproj
dotnet build InfrastructureServices/CustomerApi/CustomerApi.csproj

# Run services
dotnet run --project PaymentOrchestratorService/PaymentOrchestratorService.csproj
dotnet run --project PaymentConsumers/CheckService/CheckService.csproj
dotnet run --project PaymentConsumers/CardService/CardService.csproj
dotnet run --project InfrastructureServices/LedgerService/LedgerService.csproj
dotnet run --project InfrastructureServices/NotificationService/NotificationService.csproj
dotnet run --project InfrastructureServices/CustomerApi/CustomerApi.csproj

# Docker builds
docker build -t payment-orchestrator -f PaymentOrchestratorService/Dockerfile .
docker build -t check-service -f CheckService/Dockerfile .
docker build -t card-service -f CardService/Dockerfile .
docker build -t ledger-service -f LedgerService/Dockerfile .
docker build -t notification-service -f NotificationService/Dockerfile .
docker build -t customer-api -f InfrastructureServices/CustomerApi/Dockerfile .

# Run all tests
dotnet test PaymentProcessorApp.sln

# Run specific test projects
dotnet test PaymentConsumers/CheckService.Tests/CheckService.Tests.csproj
dotnet test PaymentConsumers/CardService.Tests/CardService.Tests.csproj
dotnet test InfrastructureServices/LedgerService.Tests/LedgerService.Tests.csproj
dotnet test InfrastructureServices/NotificationService.Tests/NotificationService.Tests.csproj

# Run a single test by name
dotnet test PaymentConsumers/CheckService.Tests/CheckService.Tests.csproj --filter "FullyQualifiedName~PaymentStatusProducerTests.PublishPaymentStatusAsync_Success"
```

## Architecture

This is a .NET 10.0 microservices solution for processing payments via multiple methods (checks, cards) using event-driven architecture with Apache Kafka.

### Service Overview

```
┌──────────────┐
│   Client     │
└──────┬───────┘
       │ HTTP POST
       ▼
┌─────────────────────────┐
│ PaymentOrchestratorAPI  │ (REST API - Entry Point)
│  - Validate requests    │
│  - Generate payment IDs │
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
│  - payment-status       │
│  - payment-events       │
│  - notification-events  │
└───┬─────────────────┬───┘
    │                 │
    ▼                 ▼
┌──────────┐    ┌──────────┐
│ Check    │    │  Card    │
│ Service  │    │ Service  │
│ (Worker) │    │ (Worker) │
└────┬─────┘    └────┬─────┘
     │               │
     └───────┬───────┘
             │
             ▼
    ┌────────────────┐
    │ payment-status │ Kafka Topic
    └────┬───────────┘
         │
         ├─────────────────┐
         ▼                 ▼
┌─────────────────┐  ┌─────────────────┐
│ Ledger Service  │  │ Notification    │
│ (Worker)        │  │ Service         │
│ - DynamoDB      │  │ (Worker)        │
└─────────────────┘  └─────────────────┘
```

### Services

**PaymentOrchestratorService** - REST API (Entry Point)
- Accepts payment requests via HTTP endpoints
- Generates unique payment IDs
- Publishes payment requests to Kafka topics
- Endpoints: `POST /api/payments/check`, `POST /api/payments/card`, `GET /health`

**CheckService** - Background Worker
- Consumes check payment requests from Kafka
- Performs OCR on check images using Tesseract
- Uses CheckReader library for OCR parsing
- Publishes payment status to Kafka
- Infrastructure pattern: Separate Consumers and Producers folders

**CardService** - Background Worker
- Consumes card payment requests from Kafka
- Processes card payments via payment gateway
- Validates card details and amounts
- Publishes payment status to Kafka

**LedgerService** - Background Worker
- Consumes payment status messages from Kafka
- Records all payment transactions in DynamoDB
- Maintains payment history and audit trail

**NotificationService** - Background Worker
- Consumes payment status messages from Kafka
- Sends email/SMS notifications to customers
- Handles notification delivery and retries

**CustomerApi** - REST API
- Customer management service
- PostgreSQL database with Dapper ORM
- Repository pattern for data access
- Endpoints: CRUD operations for customers

### Libraries

**CheckReader** - Class Library (used by CheckService)
- OCR-based check reading using Tesseract
- `Services/CheckReader.cs` - Core service that orchestrates OCR extraction
- `Services/ICheckReader.cs` - Interface for check reading operations
- `Services/TesseractOcrEngine.cs` - Tesseract OCR implementation
- `Services/Interfaces/IOcrEngine.cs` - OCR engine abstraction
- `Domain/Check.cs` - Domain model with value objects: `Check`, `Amount`, `Micr`, `Payee`
- `Parsers/` - Regex-based parsers for extracting specific fields:
  - `MicrParser` - Extracts routing number (9 digits), account number (8-17 digits), check number
  - `AmountParser` - Extracts dollar amounts in various formats
  - `DateParser` - Extracts dates in common check formats
  - `PayeeParser` - Extracts payee name from "Pay to the order of" line

### Kafka Topics

| Topic | Producer | Consumer | Message Format |
|-------|----------|----------|----------------|
| `check-payment-requests` | PaymentOrchestratorService | CheckService | CheckPaymentRequest (JSON) |
| `card-payment-requests` | PaymentOrchestratorService | CardService | CardPaymentRequest (JSON) |
| `payment-status` | CheckService, CardService | LedgerService, NotificationService | PaymentStatus (JSON) |
| `payment-events` | LedgerService | Future consumers | PaymentEvent (JSON) |
| `notification-events` | NotificationService | Future consumers | NotificationEvent (JSON) |

### Message Flow Example (Check Payment)

1. Client → `POST /api/payments/check` → PaymentOrchestratorService
2. PaymentOrchestratorService → Kafka topic `check-payment-requests`
3. CheckService consumes message → Processes check via OCR
4. CheckService → Kafka topic `payment-status`
5. LedgerService consumes → Records transaction in DynamoDB
6. NotificationService consumes → Sends notification to customer

## Key Dependencies

### Payment Orchestrator
- **ASP.NET Core 10.0** - Web framework
- **Confluent.Kafka 2.6.1** - Kafka client library
- **Microsoft.AspNetCore.OpenApi 10.0.0** - OpenAPI documentation

### Check Service
- **Confluent.Kafka 2.6.1** - Kafka client library
- **Tesseract 5.2.0** - OCR engine for reading check images
- Tessdata files must be present in `Libraries/CheckReader/tessdata/` directory

### Card Service
- **Confluent.Kafka 2.6.1** - Kafka client library

### Ledger Service
- **Confluent.Kafka 2.6.1** - Kafka client library
- **AWSSDK.DynamoDBv2** - AWS DynamoDB client

### Notification Service
- **Confluent.Kafka 2.6.1** - Kafka client library

### Customer API
- **ASP.NET Core 10.0** - Web framework
- **Dapper 2.1.35** - Micro ORM
- **Npgsql 9.0.2** - PostgreSQL provider

## Running the System

### Prerequisites
```bash
# Start Kafka (required for all services)
docker run -d --name kafka -p 9092:9092 \
  -e KAFKA_ZOOKEEPER_CONNECT=localhost:2181 \
  confluentinc/cp-kafka:latest

# Start PostgreSQL (required for CustomerApi)
docker run -d --name postgres -p 5432:5432 \
  -e POSTGRES_PASSWORD=postgres \
  postgres:17
```

### Start Services (in order)
```bash
# 1. Start the REST API entry point
dotnet run --project PaymentOrchestratorService/PaymentOrchestratorService.csproj

# 2. Start the payment processors
dotnet run --project PaymentConsumers/CheckService/CheckService.csproj
dotnet run --project PaymentConsumers/CardService/CardService.csproj

# 3. Start the infrastructure services
dotnet run --project InfrastructureServices/LedgerService/LedgerService.csproj
dotnet run --project InfrastructureServices/NotificationService/NotificationService.csproj
dotnet run --project InfrastructureServices/CustomerApi/CustomerApi.csproj
```

## Design Patterns Used

- **Event-Driven Architecture**: Services communicate via Kafka topics
- **Microservices**: Independent services with single responsibilities
- **Repository Pattern**: CustomerApi uses ICustomerRepository interface
- **Producer-Consumer Pattern**: Kafka producers and consumers
- **Domain-Driven Design**: Rich domain models in CheckReader library
- **Dependency Injection**: Constructor injection throughout
- **Infrastructure Pattern**: Separate Consumers/Producers folders in services

## Testing Strategy

- **Unit Tests**: All services have corresponding test projects
- **Mocking**: Uses Moq for mocking dependencies (Kafka, repositories, etc.)
- **Integration Tests**: Test Kafka message flow end-to-end
- **Test Organization**: Separate test classes for each component (Producers, Consumers, Services)