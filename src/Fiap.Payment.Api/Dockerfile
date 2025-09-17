# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Fiap.Payment/src/Fiap.Payment.Api/Fiap.Payment.Api.csproj", "Fiap.Payment/src/Fiap.Payment.Api/"]
COPY ["Fiap.Payment/src/Fiap.Payment.Application/Fiap.Payment.Application.csproj", "Fiap.Payment/src/Fiap.Payment.Application/"]
COPY ["Fiap.Payment/src/Fiap.Contracts.Events/Fiap.Contracts.Events.csproj", "Fiap.Payment/src/Fiap.Contracts.Events/"]
COPY ["Fiap.Payment/src/Fiap.Payment.Domain/Fiap.Payment.Domain.csproj", "Fiap.Payment/src/Fiap.Payment.Domain/"]
COPY ["Fiap.Payment/src/Fiap.Payment.Common/Fiap.Payment.Common.csproj", "Fiap.Payment/src/Fiap.Payment.Common/"]
COPY ["Fiap.Payment/src/Fiap.Payment.Infra.CrossCutting.IoC/Fiap.Payment.Infra.CrossCutting.IoC.csproj", "Fiap.Payment/src/Fiap.Payment.Infra.CrossCutting.IoC/"]
COPY ["Fiap.Payment/src/Fiap.Payment.Infra.Bus/Fiap.Payment.Infra.Bus.csproj", "Fiap.Payment/src/Fiap.Payment.Infra.Bus/"]
COPY ["Fiap.Payment/src/Fiap.Payment.Infra.Data/Fiap.Payment.Infra.Data.csproj", "Fiap.Payment/src/Fiap.Payment.Infra.Data/"]
COPY ["Fiap.Payment/src/Fiap.Payment.Infra.HostedService/Fiap.Payment.Infra.HostedService.csproj", "Fiap.Payment/src/Fiap.Payment.Infra.HostedService/"]
RUN dotnet restore "./Fiap.Payment/src/Fiap.Payment.Api/Fiap.Payment.Api.csproj"
COPY . .
WORKDIR "/src/Fiap.Payment/src/Fiap.Payment.Api"
RUN dotnet build "./Fiap.Payment.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Fiap.Payment.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fiap.Payment.Api.dll"]