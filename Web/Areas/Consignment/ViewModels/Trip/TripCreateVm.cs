using Base.Enum.Consignment;

namespace Web.Areas.Consignment.ViewModels.Trip;

public class TripCreateVm
{
    public TripType TripType { get; set; }
    public long FromBranchId { get; set; }
    public long ToBranchId { get; set; }
    public long DriverId { get; set; }
    public long VehicleId { get; set; }
    public DateTime? ScheduledDeparture { get; set; }
    public string? Notes { get; set; }
    public List<long> ConsignmentIds { get; set; } = new();
}
