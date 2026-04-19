using System.ComponentModel.DataAnnotations.Schema;
using Base.Entities.Interfaces;
using Base.Enum;

namespace Base.Entities;

[Table("vehicle_assignment", Schema = "Base")]
public class VehicleAssignment : BaseEntity, ISoftDelete
{
    public long VehicleId { get; set; }
    public virtual Vehicle Vehicle { get; set; }

    public long EmployeeId { get; set; }
    public virtual Employee Employee { get; set; }

    public VehicleAssignmentType AssignmentType { get; set; }
    public DateTime AssignedFrom { get; set; }
    public DateTime? AssignedTo { get; set; }
    public bool IsActive { get; set; } = true;
    public long? TripId { get; set; }
}
