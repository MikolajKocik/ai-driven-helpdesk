using System.Threading.Channels;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;

namespace ADH.Infrastructure.Services.Jira;

public sealed class JiraQueue : IJiraQueue
{
    private readonly Channel<JiraWorkItem> _queue;

    public JiraQueue()
    {
        _queue = Channel.CreateUnbounded<JiraWorkItem>();
    }

    public ValueTask QueueJiraWorkItemAsync(JiraWorkItem workItem, CancellationToken cancellationToken)
    {
        return _queue.Writer.WriteAsync(workItem);
    }

    public ValueTask<JiraWorkItem> DequeueAsync(CancellationToken cancellationToken)
    {
        return _queue.Reader.ReadAsync(cancellationToken);
    }
}
