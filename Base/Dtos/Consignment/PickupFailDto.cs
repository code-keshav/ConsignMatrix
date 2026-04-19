using Base.Enum.Consignment;

namespace Base.Dtos.Consignment;

public class PickupFailDto
{
    public long PickupTaskId { get; set; }
    public PickupFailReason FailReason { get; set; }
    public string? Remarks { get; set; }
    public bool Reschedule { get; set; } = true;
    public DateTime? RescheduleDate { get; set; }
    public PickupSlot? RescheduleSlot { get; set; }
}
