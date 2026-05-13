using System.Text.Json;
using ADH.Application.DTOs;
using MediatR;
using ADH.Application.Features.Chat.Commands;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;

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
        app.MapPost("chat/stream", async (HttpContext context, ChatRequest request, [FromServices] IMediator mediatR) =>
        {
            context.Response.ContentType = "text/event-stream";
            
            IAsyncEnumerable<StreamingChatMessageContent> stream = 
                await mediatR.Send(new ChatStreamCommand(request), context.RequestAborted);

            await foreach (StreamingChatMessageContent chunk in stream)
            {
                if (string.IsNullOrEmpty(chunk.Content)) continue;
                
                string json = JsonSerializer.Serialize(new { content = chunk.Content });
                await context.Response.WriteAsync($"data: {json}\n\n", context.RequestAborted);
                await context.Response.Body.FlushAsync(context.RequestAborted);
            }

            await context.Response.WriteAsync("data: [DONE]\n\n", context.RequestAborted);
            await context.Response.Body.FlushAsync(context.RequestAborted);
        })
        .RequireRateLimiting("ai-concurrency");
    }
}
