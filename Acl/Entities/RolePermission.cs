using System.ComponentModel.DataAnnotations.Schema;
using Base.Entities;

namespace Acl.Entities;

[Table("role_permission", Schema = "acl")]

public class RolePermission
{
    protected RolePermission()
    {
    }

    public RolePermission(string permission, Role role, Branch branch)
    {
        Permission = permission;
        Role = role;
        Branch = branch;
    }
    
    public long Id { get; protected set; }
    public string Permission { get; protected set; }
    public virtual Role Role { get; protected set; }
    public long RoleId { get; protected set; }
    public virtual Branch Branch { get; protected set; }
    public long BranchId { get; protected set; }
}