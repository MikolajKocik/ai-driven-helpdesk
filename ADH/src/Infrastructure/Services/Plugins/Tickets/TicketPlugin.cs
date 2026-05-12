using Microsoft.SemanticKernel;
using System.ComponentModel;
using Application.Interfaces;
using Application.DTOs;

namespace ADH.Infrastructure.Services.Plugins.Tickets;

public sealed class TicketPlugin
{
    private readonly IJiraQueue _ticketQueue;

    public TicketPlugin(IJiraQueue ticketQueue)
    {
        _ticketQueue = ticketQueue;
    }

    [KernelFunction("CreateTicket")]
    [Description("Creates a new standard IT support ticket. ALWAYS use this function when a user reports a technical problem, hardware issue, or needs IT help.")]    
    public async Task<string> CreateTicket(
        [Description("A very short, concise title summarizing the user's problem (max 5-6 words). YOU must generate this based on the user's input.")] string summary,
        [Description("The full, detailed description of the user's problem. Leave it in the original language the user wrote it.")] string description,
        CancellationToken cancellationToken = default
        )
    {
        var workItem = new JiraWorkItem
        {   
            Summary = summary,
            Description = description
        };

        await _ticketQueue.QueueJiraWorkItemAsync(workItem, cancellationToken);
        return $"Ticket {workItem.Id} created.";
    }
}
