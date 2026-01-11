# Customer API

A RESTful API for managing customer data with PostgreSQL database and Dapper ORM.

## Architecture

- **Repository Pattern**: Clean separation between data access and business logic
- **Dapper**: Lightweight ORM for efficient database queries
- **PostgreSQL**: Relational database for customer data storage

## Prerequisites

- .NET 10.0 SDK
- PostgreSQL 12 or higher
- Docker (optional, for running PostgreSQL in a container)

## Database Setup

### Option 1: Using Docker

Run PostgreSQL in a container:

```bash
docker run --name postgres-payment-processor \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=payment_processor \
  -p 5432:5432 \
  -d postgres:16
```

### Option 2: Local PostgreSQL Installation

Ensure PostgreSQL is running and create the database:

```sql
CREATE DATABASE payment_processor;
```

### Initialize Database Schema

Connect to the database and run the initialization script:

```bash
psql -U postgres -d payment_processor -f Database/init.sql
```

Or using Docker:

```bash
docker exec -i postgres-payment-processor psql -U postgres -d payment_processor < Database/init.sql
```

## Configuration

Update the connection string in `appsettings.json` or `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "PostgresConnection": "Host=localhost;Port=5432;Database=payment_processor;Username=postgres;Password=your_password"
  }
}
```

## Running the API

```bash
dotnet run --project InfrastructureServices/CustomerApi/CustomerApi.csproj
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

## API Endpoints

### Get Customer by ID
```http
GET /api/customers/{customerId}
```

**Response:**
```json
{
  "customerId": "11111111-1111-1111-1111-111111111111",
  "email": "john.doe@example.com",
  "name": "John Doe",
  "phone": "+1-555-0101"
}
```

### Get All Customers
```http
GET /api/customers
```

**Response:**
```json
[
  {
    "customerId": "11111111-1111-1111-1111-111111111111",
    "email": "john.doe@example.com",
    "name": "John Doe",
    "phone": "+1-555-0101"
  }
]
```

### Create Customer
```http
POST /api/customers
Content-Type: application/json

{
  "customerId": "44444444-4444-4444-4444-444444444444",
  "email": "new.customer@example.com",
  "name": "New Customer",
  "phone": "+1-555-0104"
}
```

**Response:** `201 Created` with created customer in response body

### Update Customer
```http
PUT /api/customers/{customerId}
Content-Type: application/json

{
  "customerId": "44444444-4444-4444-4444-444444444444",
  "email": "updated.email@example.com",
  "name": "Updated Name",
  "phone": "+1-555-0105"
}
```

**Response:** `204 No Content`

### Delete Customer
```http
DELETE /api/customers/{customerId}
```

**Response:** `204 No Content`

## Project Structure

```
CustomerApi/
├── Database/
│   └── init.sql              # Database schema and seed data
├── Models/
│   └── Customer.cs           # Customer domain model
├── Repositories/
│   ├── ICustomerRepository.cs    # Repository interface
│   └── CustomerRepository.cs     # Dapper implementation
├── Program.cs                # API endpoints and DI configuration
└── appsettings.json          # Configuration
```

## Technologies

- **ASP.NET Core 10.0**: Web framework
- **Dapper 2.1.35**: Micro ORM
- **Npgsql 9.0.2**: PostgreSQL driver for .NET
- **OpenAPI**: API documentation

## Development

### Building
```bash
dotnet build
```

### Running Tests
```bash
dotnet test
```

## OpenAPI Documentation

In development mode, OpenAPI documentation is available at:
- `/openapi/v1.json`