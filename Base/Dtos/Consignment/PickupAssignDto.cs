namespace Base.Dtos.Consignment;

public class PickupAssignDto
{
    public long PickupTaskId { get; set; }
    public long DriverId { get; set; }
    public long VehicleId { get; set; }
}

public class PickupBulkAssignDto
{
    public List<long> PickupTaskIds { get; set; } = new();
    public long DriverId { get; set; }
    public long VehicleId { get; set; }
}
