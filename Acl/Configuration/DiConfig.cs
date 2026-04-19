using Acl.Helper;
using Acl.Helper.Interface;
using Acl.Repo;
using Acl.Repo.Interfaces;
using Acl.Services;
using Acl.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Acl.Configuration;

public static class DiConfig
{
    public static IServiceCollection ConfigureAcl(this IServiceCollection service) =>
        service.AddTransient<IRolePermissionRepo, RolePermissionRepo>()
            .AddTransient<IRolePermissionService, RolePermissionService>()
            .AddTransient<IPermissionChecker, PermissionChecker>()
            .AddTransient<IPermissionProvider, PermissionProvider>()
            .AddTransient<IAuthorizationHandler, PermissionHandler>();
    
}