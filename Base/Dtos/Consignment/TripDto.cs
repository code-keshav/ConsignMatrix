using Base.Enum.Consignment;

namespace Base.Dtos.Consignment;

public class TripCreateDto
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

public class TripReceiveItemDto
{
    public long TripId { get; set; }
    public long ConsignmentId { get; set; }
    public TripReceiveAction Action { get; set; }
    public string? Remarks { get; set; }
}

public class MarkDeliveredDto
{
    public long TripId { get; set; }
    public long ConsignmentId { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}

public class MarkDeliveryFailedDto
{
    public long TripId { get; set; }
    public long ConsignmentId { get; set; }
    public DeliveryFailReason FailReason { get; set; }
    public string? Remarks { get; set; }
}
