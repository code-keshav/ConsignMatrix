using Newtonsoft.Json;

namespace Acl
{
    public class PermissionJsonParser
    {
        public static List<Permission> ParseFlat(string json)
        {
            var permissionVos = JsonConvert.DeserializeObject<List<PermissionVo>>(json);
            var list = permissionVos.SelectMany(x => x.Flat()).ToList();
            return list;
        }

        public static List<Permission> ParseForest(string json)
        {
            var permissionVos = JsonConvert.DeserializeObject<List<PermissionVo>>(json);
            var list = permissionVos.SelectMany(x => x.Forest()).ToList();
            return list;
        }

        public static List<PermissionVo> ParseTree(string json)
        {
            var permissionVos = JsonConvert.DeserializeObject<List<PermissionVo>>(json);
            var list = permissionVos.ToList();
            return list;
        }
    }
}