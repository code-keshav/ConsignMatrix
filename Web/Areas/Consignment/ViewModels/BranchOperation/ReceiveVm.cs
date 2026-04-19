namespace Web.Areas.Consignment.ViewModels.BranchOperation;

public class ReceiveVm
{
    public long ConsignmentId { get; set; }
    public string? Remarks { get; set; }
}

public class BulkReceiveVm
{
    public List<long> ConsignmentIds { get; set; } = new();
    public string? Remarks { get; set; }
}
