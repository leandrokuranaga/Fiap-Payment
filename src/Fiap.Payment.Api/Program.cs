using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Fiap.Payment.Api.Extensions;
using Fiap.Payment.Api.Middlewares;
using Fiap.Payment.Infra.CrossCutting.IoC;
using Fiap.Payment.Infra.Utils;
using HealthChecks.ApplicationStatus.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

builder.Configuration.Sources.Clear();

var awsRegion = Environment.GetEnvironmentVariable("AWS_REGION") ?? "sa-east-1";
var awsOptions = new AWSOptions { Region = RegionEndpoint.GetBySystemName(awsRegion) };
builder.Services.AddDefaultAWSOptions(awsOptions);

builder.Configuration.AddSystemsManager(src =>
{
    src.Path = "/myapp/prod/";             
    src.Optional = true;                 
    src.ReloadAfter = TimeSpan.FromMinutes(5);
    src.AwsOptions = awsOptions;
});

if (env.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true, reloadOnChange: true);
}

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
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

var conn = builder.Configuration.GetSection(nameof(ConnectionStrings)).Get<ConnectionStrings>()?.DefaultConnection;
builder.Services.AddHealthChecks().AddMySql(conn!, "mysql").AddApplicationStatus();

builder.Services.AddGlobalCorsPolicy();
builder.Services.AddApiVersioningConfiguration();
builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwaggerDocumentation();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapHealthChecks("/health");
app.MapControllers();
app.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }

public sealed class StripeOptions
{
    public string? ApiKey { get; set; }
    public string? WebhookSecret { get; set; }
}
