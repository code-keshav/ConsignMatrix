namespace Acl.Helper.Interface;

public interface IPermissionProvider
{
    List<Permission> GetFlatPermissions();
    List<Permission> GetPermissionForest();
    List<PermissionVo> GetPermissionTree();
    List<string> GetLeafPermissionsValue();
    List<string> GetLeafPermissionsKey();
    List<Permission> GetLeafPermissions();
    Dictionary<string, string> GetLeafPermissionsDictionary();
}