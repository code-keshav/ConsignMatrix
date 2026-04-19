namespace Web.Areas.Acl.ViewModels;

public class RoleVm
{
    public long Id { get; set; }
    public string Name { get; set; }
    public long? Priority { get; set; }
    public string BranchCode { get; set; }
    public string BranchName { get; set; }
    public bool IsGlobal { get; set; }
}

public class RoleEditVm : RoleVm
{
    public long Id { get; set; }
}