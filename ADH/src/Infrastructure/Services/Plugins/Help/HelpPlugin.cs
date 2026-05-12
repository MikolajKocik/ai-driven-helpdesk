using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ADH.Application.Interfaces;

namespace ADH.Infrastructure.Services.Plugins.Help;

public sealed class HelpPlugin
{
    private readonly IHelpArticleRepository _articleRepo;

    public HelpPlugin(IHelpArticleRepository articleRepo)
    {
        _articleRepo = articleRepo;
    }

    [KernelFunction, Description("Searches the knowledge base for help articles.")]
    public async Task<string> SearchHelp(string query, CancellationToken cancellationToken = default)
    {
        var results = await _articleRepo.SearchByTextAsync(query, cancellationToken);
        return string.Join("\n---\n", results.Select(a => $"# {a.Title}\n{a.Content}"));
    }
}
