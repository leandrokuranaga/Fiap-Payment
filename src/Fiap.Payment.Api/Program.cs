using Datadog.Trace;
using Datadog.Trace.Configuration;
using Fiap.Payment.Api.Extensions;
using Fiap.Payment.Api.Middlewares;
using Fiap.Payment.Infra.CrossCutting.IoC;
using Fiap.Payment.Infra.Utils;
using HealthChecks.ApplicationStatus.DependencyInjection;
using Prometheus;
using System.Diagnostics.CodeAnalysis;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

#if DEBUG
builder.Configuration.AddUserSecrets<Program>(optional: true, reloadOnChange: true);
#endif

builder.Configuration.AddSystemsManager(source =>
{
    source.Path = "/myapp/prod/";
    source.Optional = true;
    source.ReloadAfter = TimeSpan.FromMinutes(5);
});

builder.Services.AddControllers();
builder.Services.AddCustomServices(builder.Configuration);
builder.Services.AddCustomMvc();

builder.Services.AddOptions<ConnectionStrings>()
    .Bind(builder.Configuration.GetSection(nameof(ConnectionStrings)))
    .ValidateDataAnnotations()
    .ValidateOnStart();

//builder.Services.AddOptions<StripeOptions>()
//    .Bind(builder.Configuration.GetSection("Stripe"))
//    .Validate(o => !string.IsNullOrWhiteSpace(o.ApiKey), "Stripe:ApiKey missing")
//    .Validate(o => !string.IsNullOrWhiteSpace(o.WebhookSecret), "Stripe:WebhookSecret missing")
//    .ValidateOnStart();

var conn = builder.Configuration.GetSection(nameof(ConnectionStrings)).Get<ConnectionStrings>();
builder.Services
    .AddHealthChecks()
    .AddMySql(conn.PaymentConnection, "mysql")
    .AddSqs(o =>
        o.AddQueue(builder.Configuration.GetSection("ServiceBus:PaymentQueueName").ToString())
    )
    .AddEventStore(conn.EventStore)
    .AddApplicationStatus();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddGlobalCorsPolicy();

builder.Services.AddApiVersioningConfiguration();

builder.Services.AddSwaggerDocumentation();

var ddSettings = TracerSettings.FromDefaultSources();
ddSettings.ServiceName = "fiap-Payment-api";
ddSettings.Environment = "development";
ddSettings.TraceEnabled = true;
Tracer.Configure(ddSettings);

builder.Logging.AddJsonConsole(options =>
{
	options.IncludeScopes = true;
	options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
});

var app = builder.Build();

var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
startupLogger.LogInformation(
    "Application started. service={Service} env={Env} version={Version}",
    Environment.GetEnvironmentVariable("DD_SERVICE") ?? "fiap-payment-api",
    Environment.GetEnvironmentVariable("DD_ENV") ?? "development", //app.Environment.EnvironmentName,
    Environment.GetEnvironmentVariable("DD_VERSION") ?? "1.0.0");

app.UseRouting();
app.UseSwaggerDocumentation();
app.UseHttpsRedirection();

app.UseCors("AllowAllOrigins");

app.UseAuthentication();
app.UseMiddleware<UnauthorizedResponseMiddleware>();
app.UseAuthorization();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();

app.UseHttpMetrics();
app.MapMetrics();

app.MapHealthChecks("/health");
app.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }

public sealed class StripeOptions
{
    public string? ApiKey { get; set; }
    public string? WebhookSecret { get; set; }
}
