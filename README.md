# Payment Processor Application

A distributed, event-driven payment processing system built with .NET 10.0, Kafka, PostgreSQL, and DynamoDB. The system supports multiple payment methods (checks, credit cards) with asynchronous processing, ledger tracking, and customer notifications.

## Architecture Overview

This solution implements a microservices architecture with event-driven communication via Apache Kafka. Each service is designed to be independently deployable and scalable.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        Payment Processing System                              │
└─────────────────────────────────────────────────────────────────────────────┘

                         ┌──────────────────────┐
                         │ PaymentOrchestrator  │
                         │     Service          │
                         │  (Entry Point)       │
                         └──────────┬───────────┘
                                    │
                    ┌───────────────┴───────────────┐
                    │        Apache Kafka           │
                    └───────────────┬───────────────┘
                                    │
              ┌─────────────────────┼─────────────────────┐
              │                     │                     │
              ▼                     ▼                     ▼
    ┌─────────────────┐   ┌─────────────────┐   ┌─────────────────┐
    │  CheckService   │   │   CardService   │   │  (Future)       │
    │  (Consumer)     │   │   (Consumer)    │   │  ACH Service    │
    └────────┬────────┘   └────────┬────────┘   └─────────────────┘
             │                     │
             │  Uses CheckReader   │
             │  Library (OCR)      │
             │                     │
             └──────────┬──────────┘
                        │
                        │ Publishes PaymentStatus
                        │
                        ▼
              ┌─────────────────────┐
              │   Apache Kafka      │
              │ (payment-status)    │
              └──────────┬──────────┘
                         │
           ┌─────────────┼─────────────┐
           │                           │
           ▼                           ▼
  ┌──────────────────┐      ┌──────────────────────┐
  │  LedgerService   │      │ NotificationService  │
  │   (Consumer)     │      │    (Consumer)        │
  │                  │      │                      │
  │  Saves to        │      │  Fetches Customer    │
  │  DynamoDB        │      │  Sends Email         │
  └──────────────────┘      └──────────┬───────────┘
                                       │
                                       │ HTTP GET
                                       ▼
                            ┌──────────────────────┐
                            │   CustomerApi        │
                            │   (REST API)         │
                            │                      │
                            │   PostgreSQL         │
                            └──────────────────────┘
```

## System Components

### 1. Payment Orchestrator Service
**Status:** In Progress  
- **Purpose:** Entry point for all payment requests (checks, cards, future methods)
- **Features:**
  - REST API endpoints for initiating payments (`/api/payments`)
  - Payment request validation (schema, required fields, anti-fraud checks)
  - Routes requests to the correct Kafka topic based on payment method
  - Correlates requests and responses using unique payment IDs
  - Tracks request/response status for clients
  - Returns HTTP status and tracking info to clients
- **Endpoints:**
  - `POST /api/payments` — Initiate a new payment (accepts check/card/ACH)
  - `GET /api/payments/{paymentId}` — Get payment status
- **Kafka Integration:**
  - Produces to: `check-payment-requests`, `card-payment-requests`
  - Consumes from: `payment-status` (for status tracking)
- **Planned Enhancements:**
  - Authentication/authorization (JWT)
  - Idempotency for payment requests
  - API documentation via OpenAPI/Swagger
  - Distributed tracing and logging

### 2. Payment Consumer Services

#### CheckService
- **Type:** Kafka Consumer → Producer
- **Purpose:** Processes check payments using OCR
- **Kafka Topics:**
  - Consumes: `check-payment-requests`
  - Produces: `payment-status`
- **Dependencies:**
  - CheckReader library (Tesseract OCR)
- **Key Features:**
  - OCR-based check reading
  - MICR line parsing (routing, account, check numbers)
  - Amount extraction
  - Payee identification
  - Success/failure status publishing

#### CardService
- **Type:** Kafka Consumer → Producer
- **Purpose:** Processes credit/debit card payments
- **Kafka Topics:**
  - Consumes: `card-payment-requests`
  - Produces: `payment-status`
- **Key Features:**
  - Card payment processing (To be implemented)
  - Payment gateway integration
  - Success/failure status publishing

### 3. Infrastructure Services

#### LedgerService
- **Type:** Kafka Consumer
- **Purpose:** Maintains payment ledger/audit trail
- **Kafka Topics:**
  - Consumes: `payment-status`
- **Database:** AWS DynamoDB
- **Key Features:**
  - Persists all payment status updates
  - Provides audit trail
  - Supports payment history queries

#### NotificationService
- **Type:** Kafka Consumer
- **Purpose:** Sends payment notifications to customers
- **Kafka Topics:**
  - Consumes: `payment-status`
- **Dependencies:**
  - CustomerApi (for customer details)
  - Email service (SMTP/SendGrid)
- **Key Features:**
  - Fetches customer email from CustomerApi
  - Sends email notifications for payment success/failure
  - Customizable email templates

#### CustomerApi
- **Type:** REST API
- **Purpose:** Customer data management
- **Database:** PostgreSQL
- **Technology:**
  - Dapper (ORM)
  - Repository Pattern
- **Endpoints:**
  - `GET /api/customers` - Get all customers
  - `GET /api/customers/{id}` - Get customer by ID
  - `POST /api/customers` - Create customer
  - `PUT /api/customers/{id}` - Update customer
  - `DELETE /api/customers/{id}` - Delete customer

### 4. Shared Libraries

#### CheckReader
- **Purpose:** OCR-based check image processing
- **Technology:** Tesseract 5.2.0
- **Features:**
  - Check image parsing
  - MICR line extraction
  - Amount parser (handles $XXX.XX formats)
  - Date parser (multiple formats)
  - Payee parser
- **Tests:** 23 unit tests with xUnit

## Data Flow

### Check Payment Flow

```
1. Payment Request
   ┌─────────────────────┐
   │ PaymentOrchestrator │
   └──────────┬──────────┘
              │ Publishes CheckPaymentRequest
              ▼
   ┌─────────────────────┐
   │      Kafka Topic    │
   │ check-payment-      │
   │    requests         │
   └──────────┬──────────┘
              │
              ▼
2. Check Processing
   ┌─────────────────────┐
   │   CheckService      │
   │  ┌───────────────┐  │
   │  │ CheckReader   │  │
   │  │  (OCR)        │  │
   │  └───────────────┘  │
   └──────────┬──────────┘
              │ Publishes PaymentStatus
              ▼
   ┌─────────────────────┐
   │      Kafka Topic    │
   │  payment-status     │
   └──────────┬──────────┘
              │
      ┌───────┴────────┐
      │                │
3a. Ledger          3b. Notification
      ▼                ▼
┌──────────┐    ┌─────────────┐
│ Ledger   │    │Notification │
│ Service  │    │  Service    │
│          │    │      │      │
│ DynamoDB │    │      │      │
└──────────┘    │      ▼      │
                │ CustomerApi │
                │  (REST)     │
                │      │      │
                │ PostgreSQL  │
                │      │      │
                │      ▼      │
                │Send Email   │
                └─────────────┘
```

### Message Formats

#### CheckPaymentRequest
```json
{
  "paymentId": "payment-123",
  "customerId": "11111111-1111-1111-1111-111111111111",
  "imageData": "<base64-encoded-check-image>"
}
```

#### PaymentStatus
```json
{
  "paymentId": "payment-123",
  "customerId": "11111111-1111-1111-1111-111111111111",
  "amount": 150.75,
  "status": "Success",
  "errorMessage": null,
  "timestamp": "2026-01-11T10:30:00Z",
  "paymentMethod": "Check"
}
```

## Technology Stack

### Core Technologies
- **.NET 10.0** - Application framework
- **C# 13** - Programming language
- **Apache Kafka** - Event streaming platform
- **PostgreSQL 16** - Relational database (Customer data)
- **AWS DynamoDB** - NoSQL database (Ledger)

### Key Libraries
- **Confluent.Kafka** - Kafka client
- **Dapper 2.1.35** - Micro ORM
- **Npgsql 9.0.2** - PostgreSQL driver
- **Tesseract 5.2.0** - OCR engine
- **xUnit** - Testing framework
- **Moq** - Mocking framework

### Development Tools
- **Docker** - Containerization
- **OpenAPI** - API documentation

## Project Structure

```
PaymentProcessorApp/
├── PaymentOrchestratorService/          # Entry point service (placeholder)
│
├── PaymentConsumers/                    # Payment method consumers
│   ├── CheckService/                    # Check payment processor
│   │   ├── Infrastructure/
│   │   │   ├── Consumers/               # PaymentRequestConsumer
│   │   │   └── Producers/               # PaymentStatusProducer
│   │   ├── Domain/                      # Domain models
│   │   └── Worker.cs                    # Background service
│   ├── CheckService.Tests/              # Unit tests (23 tests)
│   └── CardService/                     # Card payment processor
│
├── InfrastructureServices/              # Supporting services
│   ├── CustomerApi/                     # Customer REST API
│   │   ├── Database/                    # SQL scripts
│   │   ├── Repositories/                # Repository pattern
│   │   └── Models/                      # Domain models
│   ├── LedgerService/                   # Payment ledger (DynamoDB)
│   ├── LedgerService.Tests/
│   ├── NotificationService/             # Email notifications
│   └── NotificationService.Tests/
│
└── Libraries/                           # Shared libraries
    ├── CheckReader/                     # OCR check processing
    └── CheckReader.Tests/               # Unit tests
```

## Kafka Topics

| Topic | Producers | Consumers | Message Type | Purpose |
|-------|-----------|-----------|--------------|---------|
| `check-payment-requests` | PaymentOrchestrator | CheckService | CheckPaymentRequest | Check payment submissions |
| `card-payment-requests` | PaymentOrchestrator | CardService | CardPaymentRequest | Card payment submissions |
| `payment-status` | CheckService, CardService | LedgerService, NotificationService | PaymentStatus | Payment processing results |

## Prerequisites

- .NET 10.0 SDK
- Docker & Docker Compose
- Apache Kafka
- PostgreSQL 16
- AWS Account (for DynamoDB)

## Getting Started

### 1. Start Infrastructure Services

#### Start Kafka (Docker)
```bash
# Start Zookeeper
docker run -d --name zookeeper \
  -p 2181:2181 \
  confluentinc/cp-zookeeper:latest \
  -e ZOOKEEPER_CLIENT_PORT=2181

# Start Kafka
docker run -d --name kafka \
  -p 9092:9092 \
  -e KAFKA_ZOOKEEPER_CONNECT=localhost:2181 \
  -e KAFKA_ADVERTISED_LISTENERS=PLAINTEXT://localhost:9092 \
  -e KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR=1 \
  confluentinc/cp-kafka:latest
```

#### Start PostgreSQL (Docker)
```bash
docker run --name postgres-payment-processor \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=payment_processor \
  -p 5432:5432 \
  -d postgres:16

# Initialize database
docker exec -i postgres-payment-processor \
  psql -U postgres -d payment_processor < \
  InfrastructureServices/CustomerApi/Database/init.sql
```

#### Create Kafka Topics
```bash
# Create check-payment-requests topic
docker exec kafka kafka-topics --create \
  --topic check-payment-requests \
  --bootstrap-server localhost:9092 \
  --partitions 3 \
  --replication-factor 1

# Create card-payment-requests topic
docker exec kafka kafka-topics --create \
  --topic card-payment-requests \
  --bootstrap-server localhost:9092 \
  --partitions 3 \
  --replication-factor 1

# Create payment-status topic
docker exec kafka kafka-topics --create \
  --topic payment-status \
  --bootstrap-server localhost:9092 \
  --partitions 3 \
  --replication-factor 1
```

### 2. Configure AWS DynamoDB

Update `InfrastructureServices/LedgerService/appsettings.json`:
```json
{
  "AWS": {
    "Region": "us-east-1",
    "AccessKey": "your-access-key",
    "SecretKey": "your-secret-key"
  }
}
```

### 3. Build Solution

```bash
# Build entire solution
dotnet build PaymentProcessorApp.sln

# Run tests
dotnet test PaymentProcessorApp.sln
```

### 4. Run Services

Open separate terminals for each service:

```bash
# Terminal 1: CustomerApi
dotnet run --project InfrastructureServices/CustomerApi/CustomerApi.csproj

# Terminal 2: CheckService
dotnet run --project PaymentConsumers/CheckService/CheckService.csproj

# Terminal 3: CardService
dotnet run --project PaymentConsumers/CardService/CardService.csproj

# Terminal 4: LedgerService
dotnet run --project InfrastructureServices/LedgerService/LedgerService.csproj

# Terminal 5: NotificationService
dotnet run --project InfrastructureServices/NotificationService/NotificationService.csproj

# Terminal 6: PaymentOrchestrator (when implemented)
dotnet run --project PaymentOrchestratorService/PaymentOrchestratorService.csproj
```

## Configuration

### Connection Strings

Each service has its own `appsettings.json` with appropriate configuration:

- **CheckService**: Kafka bootstrap servers, consumer/producer topics
- **CardService**: Kafka bootstrap servers, consumer/producer topics
- **CustomerApi**: PostgreSQL connection string
- **LedgerService**: Kafka bootstrap servers, AWS DynamoDB credentials
- **NotificationService**: Kafka bootstrap servers, SMTP settings, CustomerApi URL

### Environment Variables

Sensitive configuration can be overridden via environment variables:
- `ConnectionStrings__PostgresConnection`
- `AWS__AccessKey`
- `AWS__SecretKey`
- `Kafka__BootstrapServers`

## Testing

### Run All Tests
```bash
dotnet test PaymentProcessorApp.sln
```

### Run Specific Test Projects
```bash
# CheckReader tests
dotnet test Libraries/CheckReader.Tests/CheckReader.Tests.csproj

# CheckService tests
dotnet test PaymentConsumers/CheckService.Tests/CheckService.Tests.csproj

# LedgerService tests
dotnet test InfrastructureServices/LedgerService.Tests/LedgerService.Tests.csproj

# NotificationService tests
dotnet test InfrastructureServices/NotificationService.Tests/NotificationService.Tests.csproj
```

### Test Coverage
- **CheckReader**: 23 unit tests
- **CheckService**: 23 unit tests
- **Total**: 46+ unit tests

## Development

### Adding a New Payment Method

1. Create new consumer service in `PaymentConsumers/`
2. Implement `PaymentRequestConsumer` to process requests
3. Implement `PaymentStatusProducer` to publish results
4. Add Kafka topic configuration
5. Add unit tests

### Debugging

Enable debug logging in `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information"
    }
  }
}
```

## Monitoring

### Kafka Consumer Lag
```bash
docker exec kafka kafka-consumer-groups \
  --bootstrap-server localhost:9092 \
  --group check-service-consumer \
  --describe
```

### Service Health Checks
Each service logs health information at startup and during operation.

## Architecture Principles

### Event-Driven Architecture
- Asynchronous communication via Kafka
- Loose coupling between services
- Scalable and resilient

### Repository Pattern
- Clean separation of data access logic
- Testable and maintainable
- Used in CustomerApi and LedgerService

### Domain-Driven Design
- Rich domain models
- Value objects (Amount, Micr, Payee)
- Clear bounded contexts

### SOLID Principles
- Single Responsibility: Each service has one purpose
- Open/Closed: Extensible for new payment methods
- Liskov Substitution: Interface-based design
- Interface Segregation: Focused interfaces (ICustomerRepository, ILedgerRepository)
- Dependency Inversion: Dependency injection throughout

## Future Enhancements

- [ ] Implement PaymentOrchestratorService REST API
- [ ] Add authentication/authorization (JWT)
- [ ] Implement card payment gateway integration
- [ ] Add ACH/bank transfer support
- [ ] Implement saga pattern for distributed transactions
- [ ] Add API gateway (Ocelot/YARP)
- [ ] Implement circuit breaker pattern
- [ ] Add distributed tracing (OpenTelemetry)
- [ ] Implement CQRS pattern
- [ ] Add GraphQL API
- [ ] Kubernetes deployment manifests
- [ ] Prometheus metrics
- [ ] Grafana dashboards

## Contributing

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

## License

MIT License - see LICENSE file for details

## Support

For questions or issues:
- Create an issue in the repository
- Contact: [Your contact information]

## Acknowledgments

- Built with .NET 10.0
- Uses Tesseract OCR for check processing
- Powered by Apache Kafka for event streaming