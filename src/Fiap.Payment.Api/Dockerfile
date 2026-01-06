# =============================================================================
# DOCKERFILE OTIMIZADO - PAYMENT API
# Imagem base: Alpine (~100MB vs ~200MB debian)
# =============================================================================

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar apenas os .csproj primeiro (melhor cache de layers)
COPY ["src/Fiap.Payment.Api/Fiap.Payment.Api.csproj", "src/Fiap.Payment.Api/"]
COPY ["src/Fiap.Payment.Application/Fiap.Payment.Application.csproj", "src/Fiap.Payment.Application/"]
COPY ["src/Fiap.Contracts.Events/Fiap.Contracts.Events.csproj", "src/Fiap.Contracts.Events/"]
COPY ["src/Fiap.Payment.Domain/Fiap.Payment.Domain.csproj", "src/Fiap.Payment.Domain/"]
COPY ["src/Fiap.Payment.Common/Fiap.Payment.Common.csproj", "src/Fiap.Payment.Common/"]
COPY ["src/Fiap.Payment.Infra.CrossCutting.IoC/Fiap.Payment.Infra.CrossCutting.IoC.csproj", "src/Fiap.Payment.Infra.CrossCutting.IoC/"]
COPY ["src/Fiap.Payment.Infra.Bus/Fiap.Payment.Infra.Bus.csproj", "src/Fiap.Payment.Infra.Bus/"]
COPY ["src/Fiap.Payment.Infra.Data/Fiap.Payment.Infra.Data.csproj", "src/Fiap.Payment.Infra.Data/"]
COPY ["src/Fiap.Payment.Infra.HostedService/Fiap.Payment.Infra.HostedService.csproj", "src/Fiap.Payment.Infra.HostedService/"]

# Restore com cache
RUN dotnet restore "src/Fiap.Payment.Api/Fiap.Payment.Api.csproj"

# Copiar código fonte
COPY . .

# Build e publish
WORKDIR "/src/src/Fiap.Payment.Api"
RUN dotnet publish "Fiap.Payment.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# =============================================================================
# Runtime stage - Imagem final otimizada
# =============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final

# Labels para metadata
LABEL maintainer="FIAP Team" \
      app="payment-api" \
      version="1.0"

# Instalar dependências necessárias e criar usuário não-root
RUN apk add --no-cache \
        icu-libs \
        tzdata \
    && addgroup -g 1000 appgroup \
    && adduser -u 1000 -G appgroup -s /bin/sh -D appuser

# Configurar timezone
ENV TZ=America/Sao_Paulo \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

WORKDIR /app

# Copiar artefatos do build
COPY --from=build --chown=appuser:appgroup /app/publish .

# Usar usuário não-root
USER appuser

EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Fiap.Payment.Api.dll"]
