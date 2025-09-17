# Acesse https://aka.ms/customizecontainer para saber como personalizar seu contêiner de depuração e como o Visual Studio usa este Dockerfile para criar suas imagens para uma depuração mais rápida.

# Esta fase é usada durante a execução no VS no modo rápido (Padrão para a configuração de Depuração)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Esta fase é usada para compilar o projeto de serviço
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Fiap.Payment.Api/Fiap.Payment.Api/Fiap.Payment.Api.csproj", "src/Fiap.Payment.Api/Fiap.Payment.Api/"]
COPY ["src/Fiap.Payment.Application/Fiap.Payment.Application.csproj", "src/Fiap.Payment.Application/"]
COPY ["src/Fiap.Contracts.Events/Fiap.Contracts.Events.csproj", "src/Fiap.Contracts.Events/"]
COPY ["src/Fiap.Payment.Domain/Fiap.Payment.Domain.csproj", "src/Fiap.Payment.Domain/"]
COPY ["src/Fiap.Payment.Common/Fiap.Payment.Common.csproj", "src/Fiap.Payment.Common/"]
COPY ["src/Fiap.Payment.Infra.CrossCutting.IoC/Fiap.Payment.Infra.CrossCutting.IoC.csproj", "src/Fiap.Payment.Infra.CrossCutting.IoC/"]
COPY ["src/Fiap.Payment.Infra.Bus/Fiap.Payment.Infra.Bus.csproj", "src/Fiap.Payment.Infra.Bus/"]
COPY ["src/Fiap.Payment.Infra.Data/Fiap.Payment.Infra.Data.csproj", "src/Fiap.Payment.Infra.Data/"]
COPY ["src/Fiap.Payment.Infra.HostedService/Fiap.Payment.Infra.HostedService.csproj", "src/Fiap.Payment.Infra.HostedService/"]
RUN dotnet restore "./src/Fiap.Payment.Api/Fiap.Payment.Api/Fiap.Payment.Api.csproj"
COPY . .
WORKDIR "/src/src/Fiap.Payment.Api/Fiap.Payment.Api"
RUN dotnet build "./Fiap.Payment.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase é usada para publicar o projeto de serviço a ser copiado para a fase final
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Fiap.Payment.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Esta fase é usada na produção ou quando executada no VS no modo normal (padrão quando não está usando a configuração de Depuração)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fiap.Payment.Api.dll"]