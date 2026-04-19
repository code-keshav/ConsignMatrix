using Microsoft.AspNetCore.Authorization;

namespace Acl.Helper;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get;}

    public PermissionRequirement(string permission) { Permission = permission; }
}