using System.Text.Json;
using ADH.Application.Interfaces;
using ADH.Core.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Tickets.Commands;

public sealed class ProcessJiraWebhookCommandHandler : IRequestHandler<ProcessJiraWebhookCommand>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ILogger<ProcessJiraWebhookCommandHandler> _logger;

    public ProcessJiraWebhookCommandHandler(
        ITicketRepository ticketRepository,
        ILogger<ProcessJiraWebhookCommandHandler> logger
        )
    {
        _ticketRepository = ticketRepository;
        _logger = logger;
    }

    public async Task Handle(ProcessJiraWebhookCommand request, CancellationToken cancellationToken)
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
                await ProcessIssueUpdatedEvent(payload, cancellationToken);
                _logger.LogInformation("Processing ticket update...");
            }
        }
        else
        {
            _logger.LogWarning("Otrzymano nieznany format webhooka.");
        }
    }

    private async Task ProcessIssueUpdatedEvent(JsonElement payload, CancellationToken cancellationToken)
    {
        try
        {
            if (payload.TryGetProperty("issue", out JsonElement issueElement))
            {
                string? jiraKey = issueElement.GetProperty("key").GetString();

                string? newStatus = issueElement
                    .GetProperty("fields")
                    .GetProperty("status")
                    .GetProperty("name")
                    .GetString();
                
                if (!string.IsNullOrEmpty(jiraKey) && !string.IsNullOrEmpty(newStatus))
                {
                    _logger.LogInformation("Ticket {JiraKey} status updated to {NewStatus} in Jira.", jiraKey, newStatus);

                    Ticket? ticket = await _ticketRepository.GetByIdAsync(Guid.Parse(jiraKey), cancellationToken);

                    if (ticket != null)
                    {
                        ticket.UpdateStatusFromWebhook(newStatus);
                        await _ticketRepository.UpdateAsync(ticket, cancellationToken);
                    }
                    else
                    {
                        _logger.LogWarning("Local ticket: {ticket} not found", jiraKey);
                    }
                }
            }
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError(ex, "Error occured while parsing JIRA payload");
        }
    }
}