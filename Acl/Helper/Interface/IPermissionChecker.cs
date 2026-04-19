using Base.Entities;

namespace Acl.Helper.Interface;

public interface IPermissionChecker
{
    Task<bool> HasPermissionAsync(User user, string permission);
    Task<Dictionary<string, object>> GetUserPermissionsAsync(User user);
}