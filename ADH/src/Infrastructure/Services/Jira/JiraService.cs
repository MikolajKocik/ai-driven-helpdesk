using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ADH.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ADH.Infrastructure.Services.Jira;

public class JiraService : IJiraService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IAppLogger<JiraService> _logger;

    public JiraService(HttpClient httpClient, IConfiguration configuration, IAppLogger<JiraService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        string? email = _configuration["Jira:Email"];
        string? apiToken = _configuration["Jira:ApiToken"];
        string? baseUrl = _configuration["Jira:BaseUrl"];

        if (!string.IsNullOrEmpty(baseUrl))
        {
            _httpClient.BaseAddress = new Uri(baseUrl);
            string authInfo = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{email}:{apiToken}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authInfo);
        }
    }

    public async Task<string> CreateIssueAsync(string summary, string description, string priority = "Medium")
    {
        try
        {
            var issueData = new
            {
                fields = new
                {
                    project = new { key = _configuration["Jira:ProjectKey"] },
                    summary = summary,
                    description = description,
                    issuetype = new { name = "Task" },
                    priority = new { name = priority }
                }
            };

            HttpResponseMessage response = await _httpClient.PostAsync("/rest/api/2/issue", 
                new StringContent(JsonSerializer.Serialize(issueData), Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                JsonDocument doc = JsonDocument.Parse(content);
                return doc.RootElement.GetProperty("key").GetString() ?? "Unknown";
            }

            _logger.LogWarning("Jira Issue Creation failed: {StatusCode}", response.StatusCode);
            return "FAILED";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during Jira issue creation.");
            return "ERROR";
        }
    }

    public async Task<string> GetIssueStatusAsync(string issueKey)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"/rest/api/2/issue/{issueKey}");
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                JsonDocument doc = JsonDocument.Parse(content);
                return doc.RootElement.GetProperty("fields").GetProperty("status").GetProperty("name").GetString() ?? "Unknown";
            }
            return "NOT_FOUND";
        }
        catch
        {
            return "ERROR";
        }
    }
}
