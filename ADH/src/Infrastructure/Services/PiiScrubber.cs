using System.Text.RegularExpressions;
using ADH.Core.Interfaces;

namespace ADH.Infrastructure.Services;

public sealed class PiiScrubber : IPiiScrubber
{
    private static readonly Regex EmailRegex = new(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex PhoneRegex = new(@"\+?[0-9]{2,4}[-.\s]?[0-9]{3,4}[-.\s]?[0-9]{3,4}", RegexOptions.Compiled);

    public string Scrub(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        string result = input;
        
        // Mask emails
        result = EmailRegex.Replace(result, "[EMAIL_REDACTED]");
        
        // Mask phones
        result = PhoneRegex.Replace(result, "[PHONE_REDACTED]");

        return result;
    }
}
