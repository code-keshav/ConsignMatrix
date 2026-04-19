namespace Web.Areas.Consignment.ViewModels.Pickup;

public class PickupAssignVm
{
    public long PickupTaskId { get; set; }
    public long DriverId { get; set; }
    public long VehicleId { get; set; }
}

public class PickupBulkAssignVm
{
    public List<long> PickupTaskIds { get; set; } = new();
    public long DriverId { get; set; }
    public long VehicleId { get; set; }
}
