using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Threading.Tasks;
using ADH.Core.Entities;
using ADH.Application.Interfaces;

namespace ADH.Infrastructure.Services.Plugins.Help;

public sealed class KnowledgeProposalPlugin
{
    private readonly IHelpArticleRepository _articleRepo;

    public KnowledgeProposalPlugin(IHelpArticleRepository articleRepo)
    {
        _articleRepo = articleRepo;
    }

    [KernelFunction, Description("Proposes a new help article.")]
    public async Task<string> ProposeArticle(string title, string content)
    {
        await _articleRepo.AddAsync(new HelpArticle { Title = title, Content = content });
        return "Article proposed.";
    }
}
