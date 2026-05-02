using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Threading.Tasks;
using ADH.Core.Interfaces;
using System.Collections.Generic;

namespace ADH.Infrastructure.Services.Plugins.Help;

public sealed class HelpPlugin
{
    private readonly IHelpArticleRepository _articleRepo;

    public HelpPlugin(IHelpArticleRepository articleRepo)
    {
        _articleRepo = articleRepo;
    }

    [KernelFunction, Description("Searches the knowledge base for help articles.")]
    public async Task<string> SearchHelp(string query)
    {
        var results = await _articleRepo.SearchByTextAsync(query);
        return string.Join("\n---\n", results.Select(a => $"# {a.Title}\n{a.Content}"));
    }
}
