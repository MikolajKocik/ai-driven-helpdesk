using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using ADH.Infrastructure.Persistence;
using ADH.Application.Interfaces;
using ADH.Infrastructure.Repositories;
using ADH.Infrastructure.BackgroundServices;
using ADH.Infrastructure.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using ADH.Infrastructure.Services.Plugins.Ldap;
using ADH.Infrastructure.Services.Plugins.Help;
using ADH.Infrastructure.Services.Plugins.System;
using ADH.Infrastructure.Services.Plugins.Tickets;
using ADH.Infrastructure.Services.Plugins.Assets;
using ADH.Infrastructure.Services.AI;
using ADH.Infrastructure.Services.Identity;
using ADH.Infrastructure.Services.Assets;
using ADH.Infrastructure.Services.Jira;
using Microsoft.Extensions.AI;
using Infrastructure.BackgroundServices;
using Application.Interfaces;
using Infrastructure.Services.Jira;

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
        
        services.AddScoped<IAssetDiscoveryService, NetworkDiscoveryService>();
        services.AddScoped<IAssetDiscoveryService, LdapAssetDiscoveryService>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));

        services.AddSingleton<IAiProviderManager, AiProviderManager>();
        services.AddScoped<ILdapService, LdapService>();
        services.AddSingleton<IPiiScrubber, PiiScrubber>();
        services.AddSingleton<IPromptRenderFilter, PromptInjectionFilter>();

        services.AddHttpClient<IJiraService, JiraService>();
        
        // Register queues
        services.AddSingleton<IJiraQueue, JiraQueue>();
        services.AddSingleton<IWebhookQueue, WebhookQueue>();
        
        services.AddHostedService<UserSyncService>();
        services.AddHostedService<LdapPartnerSyncJob>();
        services.AddHostedService<TicketStatusSyncJob>();
        services.AddHostedService<AiModelHealthCheckJob>();
        services.AddHostedService<InfrastructureSelfHealingJob>();
        services.AddHostedService<SlaEnforcementJob>();
        services.AddHostedService<AssetDiscoveryJob>();
        services.AddHostedService<FileIndexerService>();
        services.AddHostedService<JiraWebhookProcessor>();

        return services;
    }

    /// <summary>
    /// Adds AI-related services and plugins to the kernel.
    /// </summary>
    #pragma warning disable CS0618
    public static IServiceCollection AddAiServices(this IServiceCollection services)
    {
        services.AddHttpClient("OllamaClient", (sp, client) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var baseUrl = configuration["AI:Ollama:BaseUrl"] ?? "http://localhost:11434";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromMinutes(10);
        });

        services.AddTransient<TicketPlugin>();
        services.AddTransient<HelpPlugin>();
        services.AddTransient<LdapSearchPlugin>();
        services.AddTransient<LdapAccountPlugin>();
        services.AddTransient<LdapUserManagementPlugin>();
        services.AddTransient<SystemHealthPlugin>();
        services.AddTransient<KnowledgeProposalPlugin>();
        services.AddTransient<AssetPlugin>();
        services.AddTransient<LocalAutomationPlugin>();
        services.AddTransient<AssetDiscoveryPlugin>();

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
                kernelBuilder.AddOllamaEmbeddingGenerator(modelId: "nomic-embed-text", httpClient: ollamaClient);
            }
            
            kernelBuilder.Plugins.AddFromObject(sp.GetRequiredService<TicketPlugin>());
            kernelBuilder.Plugins.AddFromObject(sp.GetRequiredService<HelpPlugin>());
            kernelBuilder.Plugins.AddFromObject(sp.GetRequiredService<LdapSearchPlugin>());
            kernelBuilder.Plugins.AddFromObject(sp.GetRequiredService<LdapAccountPlugin>());
            kernelBuilder.Plugins.AddFromObject(sp.GetRequiredService<LdapUserManagementPlugin>());
            kernelBuilder.Plugins.AddFromObject(sp.GetRequiredService<SystemHealthPlugin>());
            kernelBuilder.Plugins.AddFromObject(sp.GetRequiredService<KnowledgeProposalPlugin>());
            kernelBuilder.Plugins.AddFromObject(sp.GetRequiredService<AssetPlugin>());
            kernelBuilder.Plugins.AddFromObject(sp.GetRequiredService<LocalAutomationPlugin>());
            kernelBuilder.Plugins.AddFromObject(sp.GetRequiredService<AssetDiscoveryPlugin>());
            
            // Add Security Filters
            kernelBuilder.Services.AddSingleton<IPromptRenderFilter>(sp.GetRequiredService<IPromptRenderFilter>());
            
            return kernelBuilder.Build();
        });

        services.AddTransient<IChatCompletionService>(sp => 
            sp.GetRequiredService<Kernel>()
            .GetRequiredService<IChatCompletionService>());
            
        services.AddTransient<ITextEmbeddingGenerationService>(sp => 
            sp.GetRequiredService<Kernel>()
            .GetRequiredService<ITextEmbeddingGenerationService>());

        services.AddTransient<ChatOrchestratorService>();

        return services;
    }
    #pragma warning restore CS0618
}
