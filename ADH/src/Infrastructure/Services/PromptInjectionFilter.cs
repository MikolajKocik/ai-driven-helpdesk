using Microsoft.SemanticKernel;
using System;
using System.Linq;
using System.Threading.Tasks;
using ADH.Core.Interfaces;

namespace ADH.Infrastructure.Services;

/// <summary>
/// Filter to detect and block potential prompt injection attempts in AI interactions.
/// </summary>
public sealed class PromptInjectionFilter : IPromptRenderFilter
{
    private readonly IAppLogger<PromptInjectionFilter> _logger;
    private static readonly string[] InjectionKeywords = { "ignore previous instructions", "system prompt", "new instructions:", "you are now a", "bypass" };

    public PromptInjectionFilter(IAppLogger<PromptInjectionFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
    {
        await next(context);

        string renderedPrompt = context.RenderedPrompt?.ToLowerInvariant() ?? "";

        if (InjectionKeywords.Any(keyword => renderedPrompt.Contains(keyword)))
        {
            _logger.LogSecurityEvent("System", "Prompt Injection Detected", context.Function.Name);
            throw new InvalidOperationException("Security alert: Potential prompt injection detected. Operation blocked.");
        }
    }
}
