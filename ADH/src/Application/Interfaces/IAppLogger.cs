using System;

namespace ADH.Application.Interfaces;

/// <summary>
/// Domain-specific logger abstraction to provide structured logging for business and security events.
/// </summary>
public interface IAppLogger<T>
{
    void LogInfo(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(Exception ex, string message, params object[] args);
    void LogCritical(string message, params object[] args);
    void LogSecurityEvent(string userId, string action, string resource);
    void LogBusinessAction(string entityName, string entityId, string action);
}
