using System.ComponentModel.DataAnnotations.Schema;
using Base.Entities.Interfaces;
using Base.Enum.Consignment;

namespace Base.Entities.Consignment;

[Table("pickup_task", Schema = "consignment")]
public class PickupTask : BaseEntity, ISoftDelete
{
    public long ConsignmentId { get; set; }
    public virtual Consignment Consignment { get; set; }

    public DateTime PickupDate { get; set; }
    public PickupSlot PickupSlot { get; set; }

    public string PickupAddress { get; set; }
    public string ContactPhone { get; set; }
    public string? ContactName { get; set; }

    public long? AssignedDriverId { get; set; }
    public virtual Employee? AssignedDriver { get; set; }

    public long? AssignedVehicleId { get; set; }
    public virtual Vehicle? AssignedVehicle { get; set; }

    public PickupTaskStatus TaskStatus { get; set; } = PickupTaskStatus.Pending;
    public int AttemptCount { get; set; } = 0;

    public DateTime? PickupTime { get; set; }
    public decimal? VerifiedWeight { get; set; }
    public PickupFailReason? FailReason { get; set; }
    public string? Remarks { get; set; }
}
