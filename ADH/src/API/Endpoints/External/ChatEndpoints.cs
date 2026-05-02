using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using ADH.Core.Interfaces;
using ADH.Infrastructure.Services;

namespace ADH.API.Endpoints.External;

/// <summary>
/// Endpoints for the AI-powered helpdesk chat interface.
/// </summary>
public static class ChatEndpoints
{
    /// <summary>
    /// Maps chat-related endpoints.
    /// </summary>
    public static void MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("chat/stream", async (HttpContext context, ChatRequest request, ChatOrchestratorService chatService) =>
        {
            context.Response.ContentType = "text/event-stream";
            
            ChatHistory history = new ChatHistory();
            foreach (var msg in request.Messages)
            {
                if (msg.Role == "user") history.AddUserMessage(msg.Content);
                else history.AddAssistantMessage(msg.Content);
            }

            await foreach (StreamingChatMessageContent chunk in chatService.ChatStreamAsync(history))
            {
                string json = JsonSerializer.Serialize(new { content = chunk.Content });
                await context.Response.WriteAsync($"data: {json}\n\n");
                await context.Response.Body.FlushAsync();
            }

            await context.Response.WriteAsync("data: [DONE]\n\n");
            await context.Response.Body.FlushAsync();
        })
        .RequireRateLimiting("ai-concurrency");
    }
}

public record ChatMessage(string Role, string Content);
public record ChatRequest(List<ChatMessage> Messages);
