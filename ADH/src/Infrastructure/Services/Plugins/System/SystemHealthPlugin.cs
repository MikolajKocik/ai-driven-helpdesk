using Microsoft.SemanticKernel;
using System.ComponentModel;
using ADH.Core.Interfaces;

namespace ADH.Infrastructure.Services.Plugins.System;

public sealed class SystemHealthPlugin
{
    private readonly IAiProviderManager _aiManager;

    public SystemHealthPlugin(IAiProviderManager aiManager)
    {
        _aiManager = aiManager;
    }

    [KernelFunction, Description("Checks if the AI service is healthy.")]
    public string GetAiStatus() => _aiManager.IsOllamaHealthy ? "Healthy" : "Failing over to Cloud";
}
