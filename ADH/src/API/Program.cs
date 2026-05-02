using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ADH.Infrastructure.DependencyInjection;
using ADH.API.Middleware;
using ADH.API.Extensions;
using ADH.API.Endpoints.Internal;
using ADH.API.Endpoints.External;
using ADH.Application.Validators;
using FluentValidation;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Add core services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddHttpClient();

// Custom Extensions
builder.Services.AddAppApiVersioning();
builder.Services.AddAppAuthentication(builder.Configuration);
builder.Services.AddAppRateLimiting();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAiServices();

// Register Validators
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

WebApplication app = builder.Build();

// Configure Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<PerformanceLoggingMiddleware>();

// API Versioning Group
ApiVersionSet apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .ReportApiVersions()
    .Build();

RouteGroupBuilder versionedGroup = app.MapGroup("api/v{version:apiVersion}")
    .WithApiVersionSet(apiVersionSet)
    .RequireRateLimiting("fixed-api");

// Map Endpoints
versionedGroup.MapAuthEndpoints();
versionedGroup.MapTicketEndpoints();
versionedGroup.MapChatEndpoints();
versionedGroup.MapHelpArticleEndpoints();
versionedGroup.MapStatsEndpoints();

app.Run();

public partial class Program { }
