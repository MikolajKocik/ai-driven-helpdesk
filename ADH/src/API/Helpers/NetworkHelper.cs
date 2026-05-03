namespace ADH.API.Helpers;

public static class NetworkHelper
{
    public static readonly string[] AllowedOrigins = new[]
    {
        "http://localhost:3000",
        "http://127.0.0.1:3000",
        "http://[::1]:3000",
        "http://localhost:5173",
        "http://127.0.0.1:5173"
    };
}
