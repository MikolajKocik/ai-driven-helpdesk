using System.Threading.Tasks;

namespace ADH.Application.Interfaces;

public interface IJiraService
{
    Task<string> CreateIssueAsync(string summary, string description, CancellationToken cancellationToken, string priority = "Medium");
    Task<string> GetIssueStatusAsync(string issueKey, CancellationToken cancellationToken);
}
