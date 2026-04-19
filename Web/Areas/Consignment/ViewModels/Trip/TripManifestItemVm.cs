using Base.Enum.Consignment;

namespace Web.Areas.Consignment.ViewModels.Trip;

public class TripManifestItemVm
{
    public long ConsignmentId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public string DestinationBranchName { get; set; } = string.Empty;
    public decimal ChargeableWeight { get; set; }
    public int PackageCount { get; set; }
    public DateTime LoadedAt { get; set; }
    public ConsignmentStatusType? CurrentStatus { get; set; }
    public string? CurrentStatusLabel { get; set; }
}
