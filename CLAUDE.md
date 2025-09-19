# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Commands

### Building and Testing
- **Build solution**: `dotnet build Fiap.Payment.slnx --configuration Release`
- **Restore dependencies**: `dotnet restore Fiap.Payment.slnx`
- **Run tests**: `dotnet test Fiap.Payment.slnx --configuration Release --verbosity normal`
- **Run single test project**: `dotnet test test/Fiap.Payment.Unit.Tests/Fiap.Payment.Unit.Tests.csproj`

### Working with .slnx Format
The project uses .slnx format (Visual Studio solution format). Commands work directly with `Fiap.Payment.slnx`.
For tools requiring traditional .sln files, create a temporary one:
```bash
dotnet new sln -n Fiap.Payment
find . -name "*.csproj" -exec dotnet sln Fiap.Payment.sln add {} \;
```

### Running the API
- **Start API**: Navigate to `src/Fiap.Payment.Api/` and run `dotnet run`
- **Health check**: Available at `/health` endpoint

### Local Development with Stripe
For local testing with Stripe webhooks:
1. Install Stripe CLI: https://docs.stripe.com/stripe-cli/install
2. Forward events to local webhook: `stripe listen --api-key <test-key> --forward-to https://localhost:7238/api/payments/v1/webhook/stripe`
3. Use test API keys from Stripe dashboard

## Architecture Overview

This is a .NET 8 payment service implementing Clean Architecture with Domain-Driven Design (DDD) patterns.

### Project Structure (Hexagonal Architecture)
The solution is organized using Hexagonal Architecture principles:

**Core (Business Logic)**
- **Domain** (`Fiap.Payment.Domain`): Domain entities, aggregates, and business rules
- **Application** (`Fiap.Payment.Application`): Use cases, services, and application logic

**Adapters**
- **Inbound** (`Fiap.Payment.Api`): Web API controllers and HTTP entry points
- **Outbound** (`Fiap.Payment.Infra.*`): External integrations and persistence

**Cross-cutting Concerns**
- **Common** (`Fiap.Payment.Common`): Shared utilities and HTTP services
- **Contracts** (`Fiap.Contracts.Events`): Event contracts for messaging
- **IoC** (`Fiap.Payment.Infra.CrossCutting.IoC`): Dependency injection configuration

### Key Infrastructure Components
- **Data Layer** (`Fiap.Payment.Infra.Data`): Entity Framework Core with MySQL
- **Message Bus** (`Fiap.Payment.Infra.Bus`): Rebus with Amazon SQS for async messaging
- **Background Services** (`Fiap.Payment.Infra.HostedService`): Long-running background tasks

### Payment Processing Flow
1. Payment request received via API (`PaymentService.GamePurchase`)
2. Validates user (CRM service) and game (FCG service)
3. Domain entity created (`Payment.Create`)
4. Payment persisted to database (`IPaymentRepository`)
5. Stripe integration creates payment link (`StripeService.PurchaseGame`)
6. Payment URL returned to client
7. Stripe webhook updates payment status and releases game to user

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