using System.Threading.Tasks;

namespace ADH.Core.Interfaces;

public interface IJiraService
{
    Task<string> CreateIssueAsync(string summary, string description, string priority = "Medium");
    Task<string> GetIssueStatusAsync(string issueKey);
}
