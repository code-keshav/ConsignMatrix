using Microsoft.Extensions.Configuration;

namespace Base.Extensions;

public static class ConfigurationExtension
{
    public static string GetVerificationUrl(this IConfiguration config)
    {
        var url =  config.GetValue<string>("VerificationUrl") ?? string.Empty;
        return url.TrimEnd('/');
    }
    
}