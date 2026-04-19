using Base.Enum.Consignment;

namespace Web.Areas.Consignment.ViewModels.Pickup;

public class PickupTaskCreateVm
{
    public long ConsignmentId { get; set; }
    public DateTime PickupDate { get; set; }
    public PickupSlot PickupSlot { get; set; }
    public string PickupAddress { get; set; }
    public string ContactPhone { get; set; }
    public string? ContactName { get; set; }

    // Pre-fill display fields (read-only)
    public string? TrackingNumber { get; set; }
    public string? SenderName { get; set; }
    public string? SenderPhone { get; set; }
    public string? SenderAddress { get; set; }
    public decimal? TotalWeight { get; set; }
    public int? PackageCount { get; set; }
}
