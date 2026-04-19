using Base.Enum.Consignment;

namespace Web.Areas.Consignment.ViewModels.Pickup;

public class PickupFailVm
{
    public long PickupTaskId { get; set; }
    public PickupFailReason FailReason { get; set; }
    public string? Remarks { get; set; }
    public bool Reschedule { get; set; } = true;
    public DateTime? RescheduleDate { get; set; }
    public PickupSlot? RescheduleSlot { get; set; }
}
