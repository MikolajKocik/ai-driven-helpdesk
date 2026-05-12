using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs;

namespace Application.Interfaces;

public interface IJiraQueue
{
    ValueTask QueueJiraWorkItemAsync(JiraWorkItem workItem, CancellationToken cancellationToken);
    ValueTask<JiraWorkItem> DequeueAsync(CancellationToken cancellationToken);
}