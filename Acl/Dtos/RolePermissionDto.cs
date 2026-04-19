using Base.Entities;

namespace Acl.Dtos;

public class RolePermissionDto
{
    public Role Role;
    public List<string> Permissions;
    public Branch Branch { get; set; }
}