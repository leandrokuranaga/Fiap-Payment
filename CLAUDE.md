# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Commands

### Building and Testing
- **Build solution**: `dotnet build *.sln --configuration Release`
- **Restore dependencies**: `dotnet restore *.sln`
- **Run tests**: `dotnet test *.sln --configuration Release --verbosity normal`
- **Run single test project**: `dotnet test test/Fiap.Payment.Unit.Tests/Fiap.Payment.Unit.Tests.csproj`

### Creating Solution File
The project uses .slnx format. To work with standard tooling, create a temporary .sln file:
```bash
dotnet new sln -n Fiap.Payment
find . -name "*.csproj" -exec dotnet sln Fiap.Payment.sln add {} \;
```

### Running the API
- **Start API**: Navigate to `src/Fiap.Payment.Api/Fiap.Payment.Api/` and run `dotnet run`
- **Health check**: Available at `/health` endpoint

## Architecture Overview

This is a .NET 8 payment service implementing Clean Architecture with Domain-Driven Design (DDD) patterns.

### Core Architecture Layers
- **API Layer** (`Fiap.Payment.Api`): Web API controllers and configuration
- **Application Layer** (`Fiap.Payment.Application`): Business logic, services, and use cases
- **Domain Layer** (`Fiap.Payment.Domain`): Domain entities, aggregates, and business rules
- **Infrastructure Layers**: Cross-cutting concerns and external integrations

### Key Infrastructure Components
- **Data Layer** (`Fiap.Payment.Infra.Data`): Entity Framework Core with MySQL
- **HTTP Services** (`Fiap.Payment.Infra.CrossCutting.Http`): External API integrations (Stripe)
- **IoC Container** (`Fiap.Payment.Infra.CrossCutting.IoC`): Dependency injection setup with comprehensive observability
- **Cache Service** (`Fiap.Payment.Infra.CrossCutting.CacheService`): Redis caching implementation
- **Hosted Services** (`Fiap.Payment.Infra.HostedService`): Background services
- **Utils** (`Fiap.Payment.Infra.Utils`): Shared utilities and helpers

### Payment Processing Flow
1. Payment request received via API (`PaymentService.GamePurchase`)
2. Domain entity created (`Payment.Create`)
3. Payment persisted to database (`IPaymentRepository`)
4. Stripe integration creates payment link (`StripeService.PurchaseGame`)
5. Payment URL returned to client

### External Integrations
- **Stripe**: Payment processing via payment links for game purchases
- **MySQL**: Primary database using Pomelo provider
- **Redis**: Caching layer via StackExchange.Redis
- **Message Bus**: Rebus with Amazon SQS for async messaging

### Observability Stack
- **OpenTelemetry**: Distributed tracing with Jaeger and Prometheus exporters
- **Serilog**: Structured logging with Loki and console sinks
- **Health Checks**: MySQL and application status monitoring

### Testing
- **xUnit**: Test framework with coverlet for code coverage
- Test structure organized by layers: API, Application, and Domain tests
- CI/CD validates all tests before merging to main/dev branches

### Configuration
- User secrets enabled for API project
- Configuration follows .NET options pattern with validation
- Connection strings and external service configurations in appsettings

### Key Dependencies
- **ABP Framework**: Domain-driven design base classes
- **FluentValidation**: Request validation
- **Swashbuckle**: API documentation
- **Entity Framework Core**: ORM with migration tools