using Acl.Configuration;
using Base.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Web.Generator;
using Web.Helpers;

namespace Web.Configuration;

public static class DiConfig
{
    public static IServiceCollection ConfigureAppDi(this IServiceCollection services)
    {
        return services
            .AddScoped<DbContext, AppDbContext>()
            .AddTransient<IJwtTokenGenerator, JwtTokenGenerator>()
            .ConfigureBase()
            .ConfigureMisc()
            .ConfigureAcl();
    }

    private static IServiceCollection ConfigureMisc(this IServiceCollection services)
    =>
        services.AddTransient<INotificationHelper, NotificationHelper>()
        .AddSingleton<IAuthorizationPolicyProvider, CustomAuthorizationPolicyProvider>();
    
    
}