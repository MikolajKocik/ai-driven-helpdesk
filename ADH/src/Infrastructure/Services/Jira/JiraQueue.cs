using System.Threading.Channels;
using System.Threading.Tasks;

namespace ADH.Application.Interfaces;

public record JiraWorkItem(string Summary, string Description);

public interface IJiraQueue
{
    ValueTask QueueJiraWorkItemAsync(JiraWorkItem workItem);
    ValueTask<JiraWorkItem> DequeueAsync(CancellationToken cancellationToken);
}

public sealed class JiraQueue : IJiraQueue
{
    private readonly Channel<JiraWorkItem> _queue;

    public JiraQueue()
    {
        _queue = Channel.CreateUnbounded<JiraWorkItem>();
    }

    public ValueTask QueueJiraWorkItemAsync(JiraWorkItem workItem)
    {
        return _queue.Writer.WriteAsync(workItem);
    }

    public ValueTask<JiraWorkItem> DequeueAsync(CancellationToken cancellationToken)
    {
        return _queue.Reader.ReadAsync(cancellationToken);
    }
}
