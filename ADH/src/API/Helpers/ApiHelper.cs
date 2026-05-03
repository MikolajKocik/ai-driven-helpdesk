namespace ADH.API.Helpers;

public static class ApiHelper
{
    public const int MajorVersion = 1;
    public const int MinorVersion = 0;
    public const string ApiVersionPrefix = "api/v{version:apiVersion}";
    public const string RateLimitPolicyName = "fixed-api";
}
