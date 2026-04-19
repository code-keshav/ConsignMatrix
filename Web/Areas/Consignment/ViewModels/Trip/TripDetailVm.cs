using Base.Enum.Consignment;

namespace Web.Areas.Consignment.ViewModels.Trip;

public class TripDetailVm
{
    public long Id { get; set; }
    public string TripNumber { get; set; } = string.Empty;
    public TripType TripType { get; set; }
    public long FromBranchId { get; set; }
    public string FromBranchName { get; set; } = string.Empty;
    public long ToBranchId { get; set; }
    public string ToBranchName { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    public string DriverPhone { get; set; } = string.Empty;
    public string VehicleNumber { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public TripStatus TripStatus { get; set; }
    public int TotalConsignments { get; set; }
    public decimal TotalWeight { get; set; }
    public DateTime? ScheduledDeparture { get; set; }
    public DateTime? ActualDeparture { get; set; }
    public string? Notes { get; set; }
    public DateTime RecDate { get; set; }

    public long CurrentBranchId { get; set; }

    public List<TripManifestItemVm> Manifest { get; set; } = new();
}
