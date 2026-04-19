using Acl.Helper.Interface;
using Acl.Repo.Interfaces;
using Base.Constants;
using Base.Entities;
using Base.Repo.Interfaces;

namespace Acl.Helper;

public class PermissionChecker : IPermissionChecker
{
    private List<string> _allowedPermissionList = new List<string>();
    private readonly List<string> _allPermissionList;
    private readonly IUow _uow;
    private readonly IPermissionProvider _permissionProvider;
    private readonly IRolePermissionRepo _rolePermissionRepo;
    readonly IUserRoleRepo _userRoleRepo;

    public PermissionChecker(IUow uow, IPermissionProvider permissionProvider, IRolePermissionRepo rolePermissionRepo, IUserRoleRepo userRoleRepo)
    {
        _uow = uow;
        _permissionProvider = permissionProvider;
        _allPermissionList = permissionProvider.GetLeafPermissionsValue();
        _rolePermissionRepo = rolePermissionRepo;
        _userRoleRepo = userRoleRepo;
    }

    public async Task<bool> HasPermissionAsync(User user, string permission)
    {
        var allowedPermissions = new List<string>()
        {
            "/admin/Home/Index"
        };
        
        if (user == null) return true;
        if (user.IsSuperAdmin() || user.IsAdmin() || user.IsBranchAdmin()) return true;
        if (_allPermissionList.All(x => x != permission)) return true;
        if (_allowedPermissionList.Count == 0)
        {
            await FillPermissionOfUser(user);
        }

        return _allowedPermissionList.Any(x => x == permission);
    }

    private async Task FillPermissionOfUser(User user)
    {
        if (user.IsSuperAdmin() || user.IsAdmin())
        {
            _allowedPermissionList = _allPermissionList;
        }
        else
        {
            long branchId = user.BranchId;
            var userRole = await _userRoleRepo.FindSingleAsync(a => a.UserId == user.Id);

            var permissions = new List<string>();
            if (userRole != null)
            {
                branchId = userRole.Role.IsGlobal ? (long)IdConstants.MainBranchId : user.BranchId;
                permissions = await _rolePermissionRepo.GetPermissions(userRole.RoleId, branchId);
            }

            // var rolePermissionList = await _rolePermissionRepo.GetRolePermissionList(user);
            _allowedPermissionList = permissions;
        }
    }

    public async Task<Dictionary<string, object>> GetUserPermissionsAsync(User user)
    {
        var permissionDict = new Dictionary<string, object>();

        if (!_allowedPermissionList.Any())
        {
            await FillPermissionOfUser(user);
        }

        var leafPermissions = _permissionProvider.GetLeafPermissions();
        foreach (var leafPermission in leafPermissions)
        {
            if (_allowedPermissionList.Contains(leafPermission.Value) && !permissionDict.ContainsKey(leafPermission.resource))
            {
                permissionDict.Add(leafPermission.resource, leafPermission.Value);
            }
        }

        return permissionDict;
    }
}