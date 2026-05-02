using System;

namespace ADH.Core.Interfaces;

/// <summary>
/// Provides access to the currently authenticated user's information.
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    bool IsAuthenticated { get; }
}
