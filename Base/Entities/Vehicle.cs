using System.ComponentModel.DataAnnotations.Schema;
using Base.Entities.Interfaces;
using Base.Enum;

namespace Base.Entities;

[Table("vehicle", Schema = "Base")]
public class Vehicle : BaseEntity, ISoftDelete
{
    public string VehicleNumber { get; set; }
    public VehicleType VehicleType { get; set; }
    public VehicleOwnershipType OwnershipType { get; set; }
    public decimal MaxWeightCapacity { get; set; }
    public decimal MaxVolumeCapacity { get; set; }
    public bool SupportsFragile { get; set; }
    public bool HasColdStorage { get; set; }
    public VehicleStatus VehicleStatus { get; set; } = VehicleStatus.Available;
    public DateTime? LastServiceDate { get; set; }
    public DateTime InsuranceExpiry { get; set; }
    public FuelType FuelType { get; set; }

    public virtual Branch? CurrentBranch { get; set; }
    public long? CurrentBranchId { get; set; }

    public virtual List<VehicleAssignment> VehicleAssignments { get; set; } = new();
}
