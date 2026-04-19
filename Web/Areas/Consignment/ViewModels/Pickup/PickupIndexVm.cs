using Base.Enum.Consignment;

namespace Web.Areas.Consignment.ViewModels.Pickup;

public class PickupIndexVm
{
    public DateTime FilterDate { get; set; } = DateTime.Today;
    public PickupTaskStatus? FilterStatus { get; set; }
    public PickupSlot? FilterSlot { get; set; }
    public long? FilterDriverId { get; set; }
    public long CurrentBranchId { get; set; }
}
