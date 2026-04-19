namespace Base.Dtos.Consignment;

public class ReceiveConsignmentDto
{
    public long ConsignmentId { get; set; }
    public string? Remarks { get; set; }
}

public class BulkReceiveDto
{
    public List<long> ConsignmentIds { get; set; } = new();
    public string? Remarks { get; set; }
}

public class SortConsignmentDto
{
    public long ConsignmentId { get; set; }
    public long DestinationBranchId { get; set; }
    public string? Remarks { get; set; }
}

public class BulkSortItem
{
    public long ConsignmentId { get; set; }
    public long DestinationBranchId { get; set; }
}

public class BulkSortDto
{
    public List<BulkSortItem> Items { get; set; } = new();
    public string? Remarks { get; set; }
}

public class BagConsignmentDto
{
    public long ConsignmentId { get; set; }
    public string? Remarks { get; set; }
}

public class BulkBagDto
{
    public List<long> ConsignmentIds { get; set; } = new();
    public string? Remarks { get; set; }
}
