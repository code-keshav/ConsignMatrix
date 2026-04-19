namespace Web.Areas.Consignment.ViewModels.BranchOperation;

public class BranchOperationIndexVm
{
    public List<BranchSelectItem> Branches { get; set; } = new();
}

public class BranchSelectItem
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
