using Base.Enum.Consignment;

namespace Web.Areas.Consignment.ViewModels.Trip;

public class TripListItemVm
{
    public long Id { get; set; }
    public string TripNumber { get; set; } = string.Empty;
    public TripType TripType { get; set; }
    public string FromBranchName { get; set; } = string.Empty;
    public string ToBranchName { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    public string VehicleNumber { get; set; } = string.Empty;
    public TripStatus TripStatus { get; set; }
    public int TotalConsignments { get; set; }
    public decimal TotalWeight { get; set; }
    public DateTime? ScheduledDeparture { get; set; }
    public DateTime RecDate { get; set; }
}
