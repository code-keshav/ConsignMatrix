using Base.Enum.Consignment;

namespace Web.Areas.Consignment.ViewModels.Consignment;

public class ConsignmentFilterVm
{
    public string? TrackingNumber { get; set; }
    public ServiceType? ServiceType { get; set; }
    public PaymentMode? PaymentMode { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public List<ConsignmentListItemVm> Consignments { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
