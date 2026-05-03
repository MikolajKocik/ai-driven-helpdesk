namespace ADH.Application.Interfaces;

/// <summary>
/// Service to identify and mask sensitive information (PII) before sending it to external or untrusted services (like AI models).
/// </summary>
public interface IPiiScrubber
{
    string Scrub(string input);
}
