using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ADH.API.Middleware;

/// <summary>
/// Middleware to monitor and log the performance of HTTP requests.
/// </summary>
public class PerformanceLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceLoggingMiddleware> _logger;

    public PerformanceLoggingMiddleware(RequestDelegate next, ILogger<PerformanceLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var timer = Stopwatch.StartNew();
        
        await _next(context);
        
        timer.Stop();
        
        if (timer.ElapsedMilliseconds > 500) 
        {
            _logger.LogWarning("Long-running request: {Method} {Path} took {Elapsed} ms", 
                context.Request.Method, context.Request.Path, timer.ElapsedMilliseconds);
        }
        else
        {
            _logger.LogInformation("Request: {Method} {Path} took {Elapsed} ms", 
                context.Request.Method, context.Request.Path, timer.ElapsedMilliseconds);
        }
    }
}
