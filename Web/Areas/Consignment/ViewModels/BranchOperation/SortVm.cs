using Base.Dtos.Consignment;

namespace Web.Areas.Consignment.ViewModels.BranchOperation;

public class SortVm
{
    public long ConsignmentId { get; set; }
    public long DestinationBranchId { get; set; }
    public string? Remarks { get; set; }
}

public class BulkSortVm
{
    public List<BulkSortItem> Items { get; set; } = new();
    public string? Remarks { get; set; }
}
