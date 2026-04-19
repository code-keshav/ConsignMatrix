namespace Web.Areas.Acl.ViewModels;

public class UserRoleUpdateVm
{
    public long UserId { get; set; }
    public List<long> RoleIds { get; set; }
}