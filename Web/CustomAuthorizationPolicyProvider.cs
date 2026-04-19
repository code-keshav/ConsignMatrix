using Acl.Helper;
using Base.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace Web;

public class CustomAuthorizationPolicyProvider: IAuthorizationPolicyProvider
{
    private readonly IHttpContextAccessor _contextAccessor;
    public CustomAuthorizationPolicyProvider(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }
    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var policy = new AuthorizationPolicyBuilder();
        policy.AddRequirements(new PermissionRequirement(policyName));
        return Task.FromResult(policy.Build());
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        var policy = new AuthorizationPolicyBuilder();
        policy.AddRequirements(new PermissionRequirement(GetPolicyNameFromUrl()));
        return Task.FromResult(policy.Build());
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        var url = _contextAccessor.HttpContext.Request.Path.Value.TrimStart('/');
        if (url.IsNullOrEmpty())
            return Task.FromResult<AuthorizationPolicy>(null);
        var policy = new AuthorizationPolicyBuilder();
        policy.AddRequirements(new PermissionRequirement(GetPolicyNameFromUrl()));
        return Task.FromResult(policy.Build());
    }

    private string GetPolicyNameFromUrl()
        => _contextAccessor.HttpContext.GetUrlPermission();
}