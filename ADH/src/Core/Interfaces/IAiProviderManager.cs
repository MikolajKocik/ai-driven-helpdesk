namespace ADH.Core.Interfaces;

public interface IAiProviderManager
{
    string CurrentProvider { get; }
    void SetProvider(string provider);
    bool IsOllamaHealthy { get; set; }
}
