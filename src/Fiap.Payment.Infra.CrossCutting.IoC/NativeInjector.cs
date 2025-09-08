using System.Diagnostics.CodeAnalysis;
using Fiap.Payment.Application.Payment.Services;
using Fiap.Payment.Application.Webhook.Services;
using Fiap.Payment.Common.Http.CRM;
using Fiap.Payment.Common.Http.FCG;
using Fiap.Payment.Domain.OutboxAggregate;
using Fiap.Payment.Domain.PaymentAggregate;
using Fiap.Payment.Domain.SeedWork;
using Fiap.Payment.Infra.CrossCutting.Http.Stripe;
using Fiap.Payment.Infra.Data;
using Fiap.Payment.Infra.Data.Repositories;
using Fiap.Payment.Infra.HostedService;
using Fiap.Payment.Infra.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Refit;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

namespace Fiap.Payment.Infra.CrossCutting.IoC;

[ExcludeFromCodeCoverage]
public static class NativeInjector
{
    public static void AddCustomServices(this IServiceCollection services, IConfiguration configuration)
    {
        AddLocalHttpClients(services, configuration);
        AddLocalServices(services, configuration);
        AddObservability(services, configuration, services.BuildServiceProvider().GetRequiredService<IWebHostEnvironment>());
        AddDatabase(services, configuration);
        AddServiceBus(services, configuration);
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

    public static void AddLocalHttpClients(this IServiceCollection services, IConfiguration configuration) 
    {
        var urlFCG = configuration["ExternalServices:FCGUrl"];
        var urlCRM = configuration["ExternalServices:CRMUrl"];

        services
            .AddRefitClient<IFCGService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(urlFCG));

        services
            .AddRefitClient<ICRMService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(urlCRM));

        services.AddScoped<IStripeService, StripeService>();
    }

    public static void AddLocalServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<INotification, Notification>();

        services.AddHostedService<OutboxProcessorService>();

        #region Repositories
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        #endregion

        #region Services
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IWebhookService, WebhookService>();
        services.AddScoped<IWebhookValidator, StripeWebhookValidator>();
        #endregion
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
                .MapFallback(configuration["ServiceBus:CRMQueueName"])));

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
            .Enrich.WithProperty("Application", "Fiap-Payment")
            .WriteTo.Console()
            .WriteTo.GrafanaLoki(
                uri: lokiUri,
                labels:
                [
                    new() { Key = "app", Value = "dotnet-api-payment" },
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
        var port = int.Parse(configuration["DistributedTracing:Jaeger:Port"]);

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