namespace Acl
{
    public class PermissionVo
    {
        public const string TypeGroup = "Group";
        public const string TypePermission = "Permission";

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string? Resource { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public List<PermissionVo> Children { get; set; }
        public bool IsSelected { get; set; }

        public List<Permission> Flat(Permission parent = null)
        {
            var permission = new Permission(DisplayName, Type == TypePermission) { parent = parent, Value = Url, resource = Resource };
            permission.Value ??= permission.resolveName();
            var items = new List<Permission>()
            {
                permission
            };
            if (Children?.Count > 0)
            {
                var flatChildren = Children.SelectMany(x => x.Flat(permission));
                items.AddRange(flatChildren);
            }

            return items;
        }


        public List<Permission> Forest(Permission parent = null)
        {
            var permission = new Permission(DisplayName, Type == TypePermission) { parent = parent, Value = Url, resource = Resource };
            permission.Value ??= permission.resolveName();
            permission.children = Children.SelectMany(x => x.Forest(permission)).ToList();
            return new List<Permission>() { permission };
        }
    }
}