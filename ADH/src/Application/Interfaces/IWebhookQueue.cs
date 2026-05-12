using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IWebhookQueue
{
    ValueTask EnqueueAsync(string payload, CancellationToken cancellationToken = default);
    ValueTask<string> DequeueAsync(CancellationToken cancellationToken);
}