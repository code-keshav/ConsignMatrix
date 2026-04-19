using Acl.Helper.Interface;
using Base.Providers.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Acl.Helper;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ICurrentUserProvider _userProvider;
    private readonly IPermissionChecker _permissionChecker;

    public PermissionHandler(ICurrentUserProvider userProvider, IPermissionChecker permissionChecker)
    {
        _userProvider = userProvider;
        _permissionChecker = permissionChecker;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (!context.User.Identity.IsAuthenticated || requirement.Permission.ToLower().Equals("/admin/login/index".ToLower()))
        {
            context.Succeed(requirement);
            return;
        }

        var user = await _userProvider.GetCurrentUser();
        if (user == null)
        {
            context.Fail();
        }

        if (await _permissionChecker.HasPermissionAsync(user, requirement.Permission))
        {
            context.Succeed(requirement);
            return;
        }

        context.Fail();
    }
}