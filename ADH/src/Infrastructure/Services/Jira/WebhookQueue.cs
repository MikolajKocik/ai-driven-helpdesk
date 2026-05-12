using System.Text.Json;
using System.Threading.Channels;
using Application.Interfaces;

namespace Infrastructure.Services.Jira;

public sealed class WebhookQueue : IWebhookQueue
{
    private readonly Channel<string> _channel;

    public WebhookQueue()
    {
        // if JIRA spamed with webhooks I decided to use Bounded bcs of out of memory ex
        var options = new BoundedChannelOptions(capacity: 5000)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _channel = Channel.CreateBounded<string>(options);
    }

    public async ValueTask EnqueueAsync(string payload, CancellationToken ct = default)
    {
        await _channel.Writer.WriteAsync(payload.ToString(), ct);
    }

    public async ValueTask<string> DequeueAsync(CancellationToken ct)
    {
        return await _channel.Reader.ReadAsync(ct);
    }
}