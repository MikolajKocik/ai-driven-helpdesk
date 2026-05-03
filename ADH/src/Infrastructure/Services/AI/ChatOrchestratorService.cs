using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.AspNetCore.SignalR;
using ADH.Infrastructure.Hubs;
using ADH.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ADH.Infrastructure.Services.AI;

/// <summary>
/// Orchestrates chat completions using the Semantic Kernel and provides throttling to protect local resources.
/// Includes PII scrubbing for enhanced security.
/// </summary>
public sealed class ChatOrchestratorService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly IPiiScrubber _piiScrubber;
    private static readonly SemaphoreSlim _aiSemaphore = new SemaphoreSlim(2, 2);

    public ChatOrchestratorService(Kernel kernel, IHubContext<ChatHub> hubContext, IPiiScrubber piiScrubber)
    {
        _kernel = kernel;
        _chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        _hubContext = hubContext;
        _piiScrubber = piiScrubber;
    }

    /// <summary>
    /// Streams chat messages from the AI assistant, ensuring that concurrent requests are limited by a semaphore.
    /// Scrubs user input for PII before processing.
    /// </summary>
    /// <param name="history">The chat history to provide context for the assistant.</param>
    /// <returns>An async enumerable of streaming chat message content.</returns>
    public async IAsyncEnumerable<StreamingChatMessageContent> ChatStreamAsync(ChatHistory history, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        PromptExecutionSettings executionSettings = new PromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };
 
        // Security: Scrub PII from the last user message
        ChatMessageContent? lastMessage = history.LastOrDefault();
        if (lastMessage != null && lastMessage.Role == AuthorRole.User && !string.IsNullOrEmpty(lastMessage.Content))
        {
            lastMessage.Content = _piiScrubber.Scrub(lastMessage.Content);
        }
 
        await _aiSemaphore.WaitAsync(cancellationToken);
        try
        {
            await _hubContext.Clients.All.SendAsync("AiStatus", "typing", cancellationToken);
            await foreach (StreamingChatMessageContent chunk in _chatCompletionService.GetStreamingChatMessageContentsAsync(history, executionSettings, _kernel, cancellationToken))
            {
                yield return chunk;
            }
            await _hubContext.Clients.All.SendAsync("AiStatus", "idle");
        }
        finally
        {
            _aiSemaphore.Release();
        }
    }
}
