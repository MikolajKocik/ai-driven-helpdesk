using ADH.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ADH.Infrastructure.Services;

public sealed class AiProviderManager : IAiProviderManager
{
    private readonly bool _isAirGapped;
    private string _currentProvider;
    public bool IsOllamaHealthy { get; set; } = true;

    public AiProviderManager(IConfiguration configuration)
    {
        _isAirGapped = configuration.GetValue<bool>("AI:AirGapped");
        _currentProvider = _isAirGapped ? "Ollama" : (configuration["AI:Provider"] ?? "Ollama");
    }

    public string CurrentProvider => _currentProvider;

    public void SetProvider(string provider)
    {
        if (_isAirGapped && provider != "Ollama")
        {
            // In Air-Gapped mode, we don't allow switching to external providers
            return;
        }
        _currentProvider = provider;
    }
}
