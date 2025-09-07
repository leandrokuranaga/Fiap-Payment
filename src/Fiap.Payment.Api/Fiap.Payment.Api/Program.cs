using Fiap.Payment.Api.Extensions;
using Fiap.Payment.Api.Middlewares;
using Fiap.Payment.Infra.CrossCutting.IoC;
using Fiap.Payment.Infra.Utils;
using HealthChecks.ApplicationStatus.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;

var awsOptions = builder.Configuration.GetAWSOptions();
builder.Services.AddDefaultAWSOptions(awsOptions);

builder.Configuration.Sources.Clear();

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
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCustomServices(builder.Configuration);
builder.Services.AddCustomMvc();

builder.Services.AddOptions<ConnectionStrings>()
    .Bind(builder.Configuration.GetSection(nameof(ConnectionStrings)))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var conn = builder.Configuration.GetSection(nameof(ConnectionStrings))
                                .Get<ConnectionStrings>()?
                                .DefaultConnection;
builder.Services
    .AddHealthChecks()
    .AddMySql(conn!, name: "mysql")
    .AddApplicationStatus();

builder.Services.AddGlobalCorsPolicy();
builder.Services.AddApiVersioningConfiguration();
builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwaggerDocumentation();

app.UseRouting();
app.UseHttpsRedirection();

app.MapHealthChecks("/health");
app.UseAuthorization();
app.MapControllers();

app.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }
