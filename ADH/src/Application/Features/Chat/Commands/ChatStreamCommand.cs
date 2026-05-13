using MediatR;
using ADH.Application.DTOs;
using Microsoft.SemanticKernel;

namespace ADH.Application.Features.Chat.Commands;

public record ChatStreamCommand(ChatRequest Request) : IRequest<IAsyncEnumerable<StreamingChatMessageContent>>;
