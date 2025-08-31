using Fiap.Payment.Api.Extensions;
using Fiap.Payment.Api.Middlewares;
using Fiap.Payment.Infra.CrossCutting.IoC;
using Fiap.Payment.Infra.Utils;
using HealthChecks.ApplicationStatus.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCustomServices(builder.Configuration);
builder.Services.AddCustomMvc();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOptions<ConnectionStrings>()
    .Bind(builder.Configuration.GetSection(nameof(ConnectionStrings)))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var serviceProvider = builder.Services.BuildServiceProvider();
var databaseConnection = serviceProvider.GetRequiredService<IOptions<ConnectionStrings>>().Value;

builder.Services
    .AddHealthChecks()
    .AddMySql(databaseConnection.DefaultConnection, name: "mysql")
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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }