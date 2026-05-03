using System.Threading.Tasks;

namespace ADH.Application.Interfaces;

public interface IJiraService
{
    Task<string> CreateIssueAsync(string summary, string description, string priority = "Medium");
    Task<string> GetIssueStatusAsync(string issueKey);
}
