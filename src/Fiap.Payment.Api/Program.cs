using Fiap.Payment.Api.Extensions;
using Fiap.Payment.Api.Middlewares;
using Fiap.Payment.Infra.CrossCutting.IoC;
using Fiap.Payment.Infra.Utils;
using HealthChecks.ApplicationStatus.DependencyInjection;
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

builder.Services.AddOptions<StripeOptions>()
    .Bind(builder.Configuration.GetSection("Stripe"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.ApiKey), "Stripe:ApiKey missing")
    .Validate(o => !string.IsNullOrWhiteSpace(o.WebhookSecret), "Stripe:WebhookSecret missing")
    .ValidateOnStart();

var conn = builder.Configuration.GetSection(nameof(ConnectionStrings)).Get<ConnectionStrings>()?.PaymentConnection;
builder.Services
    .AddHealthChecks()
    .AddMySql(conn!, "mysql")
    .AddApplicationStatus();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddGlobalCorsPolicy();

builder.Services.AddApiVersioningConfiguration();

builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

app.UseRouting();
app.UseSwaggerDocumentation();
app.UseHttpsRedirection();

app.UseCors("AllowAllOrigins");

app.UseAuthentication();
app.UseMiddleware<UnauthorizedResponseMiddleware>();
app.UseAuthorization();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();

app.MapHealthChecks("/health");
app.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }

public sealed class StripeOptions
{
    public string? ApiKey { get; set; }
    public string? WebhookSecret { get; set; }
}
