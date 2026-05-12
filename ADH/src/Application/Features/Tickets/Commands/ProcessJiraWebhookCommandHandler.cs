using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Tickets.Commands;

public class ProcessJiraWebhookCommandHandler : IRequestHandler<ProcessJiraWebhookCommand>
{
    private readonly ILogger<ProcessJiraWebhookCommandHandler> _logger;

    public ProcessJiraWebhookCommandHandler(ILogger<ProcessJiraWebhookCommandHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(ProcessJiraWebhookCommand request, CancellationToken cancellationToken)
    {
        var rawText = request.Payload;

        using JsonDocument document = JsonDocument.Parse(rawText);
        JsonElement payload = document.RootElement;
        
        if (payload.TryGetProperty("webhookEvent", out JsonElement eventElement))
        {
            string webhookEvent = eventElement.GetString() ?? "unknown";
            
            _logger.LogInformation("Webhook received from JIRA!. Event type: {WebhookEvent}", webhookEvent);

            if (webhookEvent == "jira:issue_updated")
            {
                // TODO
                _logger.LogInformation("Processing ticket update...");
            }
        }
        else
        {
            _logger.LogWarning("Otrzymano nieznany format webhooka.");
        }

        return Task.CompletedTask;
    }
}