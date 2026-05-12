using ADH.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using ADH.Infrastructure.Hubs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Application.Interfaces;
using Application.DTOs;
using ADH.Core.Entities;

namespace ADH.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that processes Jira work items from a queue.
/// </summary>
public class JiraBackgroundProcessor : BackgroundService
{
    private readonly IJiraQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly ILogger<JiraBackgroundProcessor> _logger;

    public JiraBackgroundProcessor(IJiraQueue queue, IServiceProvider serviceProvider, IHubContext<ChatHub> hubContext, ILogger<JiraBackgroundProcessor> logger)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Dequeues and processes work items until cancellation is requested.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Jira Background Processor is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                JiraWorkItem workItem = await _queue.DequeueAsync(stoppingToken);
                
                _logger.LogInformation("Processing Jira work item: {Summary}", workItem.Summary);

                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    IJiraService jiraService = scope.ServiceProvider.GetRequiredService<IJiraService>();
                    string? key = await jiraService.CreateIssueAsync(workItem.Summary, workItem.Description, stoppingToken);
                    
                    if (key != null)
                    {
                        _logger.LogInformation("Jira issue created successfully in background: {Key}", key);
                        
                        var ticketRepo = scope.ServiceProvider.GetRequiredService<ITicketRepository>();
                        Ticket? localTicket = await ticketRepo.GetByIdAsync(workItem.Id, stoppingToken);
                        
                        if (localTicket != null)
                        {
                            localTicket.LinkToExternalSystem("JIRA", key);
                            await ticketRepo.UpdateAsync(localTicket, stoppingToken);
                        }
                        
                        await _hubContext.Clients.All.SendAsync("ReceiveMessage", "System", $"New Jira Issue created: {key}");
                    }
                    else
                    {
                        _logger.LogWarning("Failed to create Jira issue in background for: {Summary}", workItem.Summary);
                        await Task.Delay(TimeSpan.FromSeconds(30));
                        await _queue.QueueJiraWorkItemAsync(workItem, stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing Jira work item");
            }
        }

        _logger.LogInformation("Jira Background Processor is stopping.");
    }
}
