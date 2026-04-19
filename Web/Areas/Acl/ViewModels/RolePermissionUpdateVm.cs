using Acl;
using Base.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.Areas.Acl.ViewModels;

public class RolePermissionUpdateVm
{
    public long? RoleId { get; set; }
    public List<string> Permissions { get; set; } = new();
    public string? RoleName { get; set; }
    public List<PermissionVo> AllPermissions { get; set; } = new();
    public List<Role> Roles { get; set; }
    public SelectList RoleOptions => new SelectList(Roles, nameof(Role.Id), nameof(Role.Name), RoleId);
    public Role? Role { get; set; }
}