using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Threading.Tasks;
using ADH.Application.Interfaces;

namespace ADH.Infrastructure.Services.Plugins.Jira;

public sealed class JiraPlugin
{
    private readonly IJiraService _jiraService;

    public JiraPlugin(IJiraService jiraService)
    {
        _jiraService = jiraService;
    }

    [KernelFunction, Description("Creates a Jira issue.")]
    public async Task<string> CreateIssue(string summary, string description) => 
        await _jiraService.CreateIssueAsync(summary, description);

    [KernelFunction, Description("Gets Jira issue status.")]
    public async Task<string> GetStatus(string key) => 
        await _jiraService.GetIssueStatusAsync(key);
}
