using MediatR;
using ADH.Infrastructure.Services.AI;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using ADH.Application.Features.Chat.Commands;

namespace ADH.Infrastructure.Features.Chat.Commands;

public sealed class ChatStreamCommandHandler : IRequestHandler<ChatStreamCommand, IAsyncEnumerable<StreamingChatMessageContent>>
{
    private readonly ChatOrchestratorService _chatService;

    public ChatStreamCommandHandler(ChatOrchestratorService chatService)
    {
        _chatService = chatService;
    }

    public async Task<IAsyncEnumerable<StreamingChatMessageContent>> Handle(ChatStreamCommand request, CancellationToken cancellationToken)
    {
        ChatHistory history = new ChatHistory();
        foreach (var msg in request.Request.Messages)
        {
            if (msg.Role == "user") history.AddUserMessage(msg.Content);
            else history.AddAssistantMessage(msg.Content);
        }

        return _chatService.ChatStreamAsync(history, cancellationToken);
    }
}
