namespace Web.Areas.Consignment.ViewModels.BranchOperation;

public class BagVm
{
    public long ConsignmentId { get; set; }
    public string? Remarks { get; set; }
}

public class BulkBagVm
{
    public List<long> ConsignmentIds { get; set; } = new();
    public string? Remarks { get; set; }
}
