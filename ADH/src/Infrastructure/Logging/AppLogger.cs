using ADH.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;

namespace ADH.Infrastructure.Logging;

public sealed class AppLogger<T> : IAppLogger<T>
{
    private readonly ILogger<T> _logger;

    public AppLogger(ILogger<T> logger)
    {
        _logger = logger;
    }

    public void LogInfo(string message, params object[] args)
    {
        _logger.LogInformation(message, args);
    }

    public void LogWarning(string message, params object[] args)
    {
        _logger.LogWarning(message, args);
    }

    public void LogError(Exception ex, string message, params object[] args)
    {
        _logger.LogError(ex, message, args);
    }

    public void LogCritical(string message, params object[] args)
    {
        _logger.LogCritical(message, args);
    }

    public void LogSecurityEvent(string userId, string action, string resource)
    {
        _logger.LogCritical("[SECURITY] User {UserId} performed {Action} on {Resource}", userId, action, resource);
    }

    public void LogBusinessAction(string entityName, string entityId, string action)
    {
        _logger.LogInformation("[BUSINESS] {EntityName} ({EntityId}) was {Action}", entityName, entityId, action);
    }
}
