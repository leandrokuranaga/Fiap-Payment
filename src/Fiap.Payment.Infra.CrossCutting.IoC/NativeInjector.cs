using System.Diagnostics.CodeAnalysis;
using Fiap.Payment.Domain.OutboxAggregate;
using Fiap.Payment.Domain.SeedWork;
using Fiap.Payment.Infra.CrossCutting.CacheService.Services;
using Fiap.Payment.Infra.Data;
using Fiap.Payment.Infra.Data.Repositories;
using Fiap.Payment.Infra.HostedService;
using Fiap.Payment.Infra.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using StackExchange.Redis;

namespace Fiap.Payment.Infra.CrossCutting.IoC;

[ExcludeFromCodeCoverage]
public static class NativeInjector
{
    public static void AddCustomServices(this IServiceCollection services, IConfiguration configuration)
    {
        AddLocalHttpClients(services, configuration);
        AddLocalServices(services, configuration);
        AddRedisDatabase(services, configuration);
        AddObservability(services, configuration, services.BuildServiceProvider().GetRequiredService<IWebHostEnvironment>());
        AddDatabase(services, configuration);
    }

    public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ConnectionStrings>()
            .Bind(configuration.GetSection(nameof(ConnectionStrings)))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<Context>(options =>
            options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21))));
    }

    public static void AddLocalHttpClients(this IServiceCollection services, IConfiguration configuration) { }

    public static void AddLocalServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<INotification, Notification>();

        services.AddHostedService<OutboxProcessorService>();

        #region Repositories
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        #endregion

        #region Services

        #endregion
    }

    public static void AddRedisDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis");
        
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.ConfigurationOptions = ConfigurationOptions.Parse(redisConnectionString);
            options.ConfigurationOptions.AbortOnConnectFail = false;
            options.ConfigurationOptions.ConnectTimeout = 10000;
            options.ConfigurationOptions.SyncTimeout = 10000;
            options.ConfigurationOptions.ConnectRetry = 5;
            options.ConfigurationOptions.ReconnectRetryPolicy = new ExponentialRetry(5000);
        });

        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            return ConnectionMultiplexer.Connect(redisConnectionString);
        });

        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                LocalCacheExpiration = TimeSpan.FromMinutes(5),
                Expiration = TimeSpan.FromMinutes(60)
            };
        });
        
        services.AddSingleton<ICacheService, CacheService.Services.CacheService>();
    }

    public static IServiceCollection AddObservability(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        ConfigureSerilog(services, configuration, environment);
        ConfigureOpenTelemetry(services, configuration);
        return services;
    }

    public static IServiceCollection AddServiceBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ServiceBus>()
            .Bind(configuration.GetSection(nameof(ServiceBus)))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddRebus(configure => configure
            .Transport(t => t.UseAmazonSQSAsOneWayClient(
                accessKeyId: configuration["ServiceBus:AccessKey"],
                secretAccessKey: configuration["ServiceBus:SecretKey"],
                regionEndpoint: Amazon.RegionEndpoint.GetBySystemName(configuration["ServiceBus:Region"])))
            .Routing(r => r.TypeBased()
                .MapFallback(configuration["ServiceBus:QueueName"])));

        return services;
    }

    private static void ConfigureSerilog(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddOptions<Utils.Serilog>()
            .Bind(configuration.GetSection(nameof(Serilog)))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var lokiUri = configuration["Serilog:WriteTo:1:Args:Uri"];

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithProperty("Application", "Fiap")
            .WriteTo.Console()
            .WriteTo.GrafanaLoki(
                uri: lokiUri,
                labels:
                [
                    new() { Key = "app", Value = "dotnet-api" },
                    new() { Key = "env", Value = environment.EnvironmentName }
                ])
            .CreateLogger();
    }

    private static void ConfigureOpenTelemetry(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DistributedTracing>()
            .Bind(configuration.GetSection(nameof(DistributedTracing)))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var serviceName = configuration["DistributedTracing:Jaeger:ServiceName"];
        var endpoint = configuration["DistributedTracing:Jaeger:Endpoint"];
        var host = configuration["DistributedTracing:Jaeger:Host"];
        var port = int.Parse(configuration["DistributedTracing:Jaeger:Port"] ?? "14268");

        services.AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(serviceName: serviceName, serviceVersion: "1.0.0"))
                .AddAspNetCoreInstrumentation()
                .AddEntityFrameworkCoreInstrumentation(options =>
                {
                    options.SetDbStatementForText = true;
                    options.SetDbStatementForStoredProcedure = true;
                })
                .AddJaegerExporter(options =>
                {
                    if (!string.IsNullOrEmpty(endpoint))
                    {
                        options.Endpoint = new Uri(endpoint);
                        options.Protocol = JaegerExportProtocol.HttpBinaryThrift;
                    }
                    else
                    {
                        options.AgentHost = host;
                        options.AgentPort = port;
                    }
                }));
    }
}