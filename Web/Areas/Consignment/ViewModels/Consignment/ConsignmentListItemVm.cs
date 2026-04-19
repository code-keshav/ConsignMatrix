using Base.Enum.Consignment;

namespace Web.Areas.Consignment.ViewModels.Consignment;

public class ConsignmentListItemVm
{
    public long Id { get; set; }
    public string TrackingNumber { get; set; }
    public string SenderName { get; set; }
    public string ReceiverName { get; set; }
    public ServiceType ServiceType { get; set; }
    public PaymentMode PaymentMode { get; set; }
    public int PackageCount { get; set; }
    public decimal ChargeableWeight { get; set; }
    public string? LatestStatus { get; set; }
    public DateTime RecDate { get; set; }
    public bool IsActive { get; set; }
}
