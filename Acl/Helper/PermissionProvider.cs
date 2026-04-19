using Acl.Helper.Interface;

namespace Acl.Helper;

public class PermissionProvider : IPermissionProvider
{
    private List<Permission> FlatPermissionList { get; set; } = new List<Permission>();
    private List<Permission> PermissionForest { get; set; } = new List<Permission>();
    private List<PermissionVo> PermissionTree { get; set; } = new List<PermissionVo>();
    

    public List<Permission> GetFlatPermissions()
    {
        if (FlatPermissionList.Count == 0) FillAllPermissions();
        return FlatPermissionList;
    }

    public List<Permission> GetPermissionForest()
    {
        if (!PermissionForest.Any())
        {
            FillAllPermissions();
        }

        return PermissionForest;
    }
    
    public List<PermissionVo> GetPermissionTree()
    {
        if (PermissionTree.Count == 0) FillAllPermissions();
        return PermissionTree;
    }

    public List<Permission> GetLeafPermissions() => GetFlatPermissions().Where(x => x.isLeaf).ToList();
    public List<string> GetLeafPermissionsKey() => GetLeafPermissions().Select(x => x.resource).ToList();
    public List<string> GetLeafPermissionsValue() => GetLeafPermissions().Select(x => x.Value).ToList();


    public Dictionary<string, string> GetLeafPermissionsDictionary()
    {
        var leafPermissions = GetLeafPermissions();
        var permissionDict = new Dictionary<string, string>();
        foreach (var leafPermission in leafPermissions)
        {
            if (!permissionDict.ContainsKey(leafPermission.resource))
            {
                permissionDict.Add(leafPermission.resource, leafPermission.Value);
            }
        }

        return permissionDict;
    }

    private void FillAllPermissions()
    {
        var rootPath = Directory.GetCurrentDirectory();
        var path = Path.Combine(rootPath, "Acl.json");

        var data = File.ReadAllText(path);
        FlatPermissionList = PermissionJsonParser.ParseFlat(data);
        PermissionForest = PermissionJsonParser.ParseForest(data);
        PermissionTree = PermissionJsonParser.ParseTree(data);
    }
}