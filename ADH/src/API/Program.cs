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
using ADH.Infrastructure.Hubs;
using ADH.API.Helpers;
using Microsoft.EntityFrameworkCore;
using ADH.Infrastructure.Persistence;

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

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(NetworkHelper.AllowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Custom Extensions
builder.Services.AddAppApiVersioning();
builder.Services.AddAppAuthentication(builder.Configuration);
builder.Services.AddAppRateLimiting();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAiServices();

// Register Validators
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

WebApplication app = builder.Build();

app.UseCors();

// Configure Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<PerformanceLoggingMiddleware>();

// API Versioning Group
ApiVersionSet apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(ApiHelper.MajorVersion, ApiHelper.MinorVersion))
    .ReportApiVersions()
    .Build();

RouteGroupBuilder versionedGroup = app.MapGroup(ApiHelper.ApiVersionPrefix)
    .WithApiVersionSet(apiVersionSet)
    .RequireRateLimiting(ApiHelper.RateLimitPolicyName);

// Map Endpoints
versionedGroup.MapAuthEndpoints();
versionedGroup.MapTicketEndpoints();
versionedGroup.MapChatEndpoints();
versionedGroup.MapHelpArticleEndpoints();
versionedGroup.MapStatsEndpoints();

// Map Third-party OAuth endpoints (without versioning)
app.MapThirdAuthEndpoints();

app.MapHub<ChatHub>("/hubs/chat");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ADH.Infrastructure.Persistence.ApplicationDbContext>();
        var userRepo = services.GetRequiredService<ADH.Application.Interfaces.IUserRepository>();
        
        await DbInitializer.SeedAsync(context, userRepo);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database initialization.");
    }
}

await app.RunAsync();

public partial class Program { }
