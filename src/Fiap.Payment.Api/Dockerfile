# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

RUN mkdir -p /opt/datadog
RUN curl -LO https://github.com/DataDog/dd-trace-dotnet/releases/download/v3.8.0/datadog-dotnet-apm_3.8.0_amd64.deb
RUN dpkg -i datadog-dotnet-apm_3.8.0_amd64.deb
RUN rm datadog-dotnet-apm_3.8.0_amd64.deb

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Fiap.Payment.Api/Fiap.Payment.Api.csproj", "src/Fiap.Payment.Api/"]
COPY ["src/Fiap.Payment.Application/Fiap.Payment.Application.csproj", "src/Fiap.Payment.Application/"]
COPY ["src/Fiap.Contracts.Events/Fiap.Contracts.Events.csproj", "src/Fiap.Contracts.Events/"]
COPY ["src/Fiap.Payment.Domain/Fiap.Payment.Domain.csproj", "src/Fiap.Payment.Domain/"]
COPY ["src/Fiap.Payment.Common/Fiap.Payment.Common.csproj", "src/Fiap.Payment.Common/"]
COPY ["src/Fiap.Payment.Infra.CrossCutting.IoC/Fiap.Payment.Infra.CrossCutting.IoC.csproj", "src/Fiap.Payment.Infra.CrossCutting.IoC/"]
COPY ["src/Fiap.Payment.Infra.Bus/Fiap.Payment.Infra.Bus.csproj", "src/Fiap.Payment.Infra.Bus/"]
COPY ["src/Fiap.Payment.Infra.Data/Fiap.Payment.Infra.Data.csproj", "src/Fiap.Payment.Infra.Data/"]
COPY ["src/Fiap.Payment.Infra.HostedService/Fiap.Payment.Infra.HostedService.csproj", "src/Fiap.Payment.Infra.HostedService/"]
RUN dotnet restore "./src/Fiap.Payment.Api/Fiap.Payment.Api.csproj"
COPY . .
WORKDIR "/src/src/Fiap.Payment.Api"
RUN dotnet build "./Fiap.Payment.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Fiap.Payment.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV CORECLR_ENABLE_PROFILING=1
ENV CORECLR_PROFILER={846F5F1C-F9AE-4B07-969E-05C26BC060D8}
ENV CORECLR_PROFILER_PATH=/opt/datadog/Datadog.Trace.ClrProfiler.Native.so
ENV DD_DOTNET_TRACER_HOME=/opt/datadog

ENTRYPOINT ["dotnet", "Fiap.Payment.Api.dll"]