using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using ADH.Infrastructure.Persistence;
using ADH.Core.Interfaces;
using ADH.Infrastructure.Repositories;
using ADH.Infrastructure.Services;
using ADH.Infrastructure.Services.Plugins;
using ADH.Infrastructure.BackgroundServices;
using ADH.Infrastructure.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ADH.Infrastructure.Services.Plugins.Ldap;
using ADH.Infrastructure.Services.Plugins.Jira;
using ADH.Infrastructure.Services.Plugins.Help;
using ADH.Infrastructure.Services.Plugins.System;
using ADH.Infrastructure.Services.Plugins.Tickets;
using ADH.Infrastructure.Services.Plugins.Assets;
using ADH.Infrastructure.Services.Jira;
using System.Net.Http;

namespace ADH.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for setting up infrastructure services in <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds core infrastructure services including database, repositories, and background services.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, x => x.UseVector()));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IHelpArticleRepository, HelpArticleRepository>();
        services.AddScoped<IAssetRepository, AssetRepository>();
        services.AddScoped<ISlaPolicyRepository, SlaPolicyRepository>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));

        services.AddSingleton<IAiProviderManager, AiProviderManager>();
        services.AddScoped<ILdapService, LdapService>();
        services.AddSingleton<IPiiScrubber, PiiScrubber>();
        services.AddSingleton<IPromptRenderFilter, PromptInjectionFilter>();

        services.AddHttpClient<IJiraService, JiraService>();
        
        services.AddHostedService<UserSyncService>();
        services.AddHostedService<LdapPartnerSyncJob>();
        services.AddHostedService<TicketStatusSyncJob>();
        services.AddHostedService<AiModelHealthCheckJob>();
        services.AddHostedService<InfrastructureSelfHealingJob>();
        services.AddHostedService<SlaEnforcementJob>();
        services.AddHostedService<FileIndexerService>();

        return services;
    }

    /// <summary>
    /// Adds AI-related services and plugins to the kernel.
    /// </summary>
    public static IServiceCollection AddAiServices(this IServiceCollection services)
    {
        services.AddTransient<TicketPlugin>();
        services.AddTransient<HelpPlugin>();
        services.AddTransient<LdapSearchPlugin>();
        services.AddTransient<LdapAccountPlugin>();
        services.AddTransient<LdapUserManagementPlugin>();
        services.AddTransient<JiraPlugin>();
        services.AddTransient<SystemHealthPlugin>();
        services.AddTransient<KnowledgeProposalPlugin>();
        services.AddTransient<AssetPlugin>();
        services.AddTransient<LocalAutomationPlugin>();

        services.AddTransient<Kernel>(sp =>
        {
            IConfiguration configuration = sp.GetRequiredService<IConfiguration>();
            IHttpClientFactory httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            IAiProviderManager aiManager = sp.GetRequiredService<IAiProviderManager>();
            
            string aiProvider = aiManager.CurrentProvider;
            
            IKernelBuilder kernelBuilder = Kernel.CreateBuilder();

            if (aiProvider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
            {
                string? apiKey = configuration["AI:OpenAI:ApiKey"];
                string? modelId = configuration["AI:OpenAI:ModelId"] ?? "gpt-4o";
                
                if (string.IsNullOrEmpty(apiKey))
                    throw new InvalidOperationException("OpenAI ApiKey is missing in configuration.");

                kernelBuilder.AddOpenAIChatCompletion(modelId: modelId, apiKey: apiKey);
                kernelBuilder.AddOpenAITextEmbeddingGeneration(modelId: "text-embedding-3-small", apiKey: apiKey);
            }
            else
            {
                HttpClient ollamaClient = httpClientFactory.CreateClient("OllamaClient");
                kernelBuilder.AddOllamaChatCompletion(modelId: "llama3.2", httpClient: ollamaClient);
                kernelBuilder.AddOllamaTextEmbeddingGeneration(modelId: "nomic-embed-text", httpClient: ollamaClient);
            }
            
            kernelBuilder.Plugins.AddFromObject(sp.GetRequiredService<TicketPlugin>());
            kernelBuilder.Plugins.AddFromObject(sp.GetRequiredService<HelpPlugin>());
            kernelBuilder.Plugins.AddFromObject(sp.GetRequiredService<LdapSearchPlugin>());
            kernelBuilder.Plugins.AddFromObject(sp.GetRequiredService<LdapAccountPlugin>());
            kernelBuilder.Plugins.AddFromObject(sp.GetRequiredService<LdapUserManagementPlugin>());
            kernelBuilder.Plugins.AddFromObject(sp.GetRequiredService<JiraPlugin>());
            kernelBuilder.Plugins.AddFromObject(sp.GetRequiredService<SystemHealthPlugin>());
            kernelBuilder.Plugins.AddFromObject(sp.GetRequiredService<KnowledgeProposalPlugin>());
            kernelBuilder.Plugins.AddFromObject(sp.GetRequiredService<AssetPlugin>());
            kernelBuilder.Plugins.AddFromObject(sp.GetRequiredService<LocalAutomationPlugin>());
            
            // Add Security Filters
            kernelBuilder.Services.AddSingleton<IPromptRenderFilter>(sp.GetRequiredService<IPromptRenderFilter>());
            
            return kernelBuilder.Build();
        });

        services.AddTransient<ChatOrchestratorService>();

        return services;
    }
}
