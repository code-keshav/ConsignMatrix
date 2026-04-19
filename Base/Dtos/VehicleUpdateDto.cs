using Base.Enum;

namespace Base.Dtos;

public class VehicleUpdateDto
{
    public string VehicleNumber { get; set; }
    public VehicleType VehicleType { get; set; }
    public VehicleOwnershipType OwnershipType { get; set; }
    public decimal MaxWeightCapacity { get; set; }
    public decimal MaxVolumeCapacity { get; set; }
    public bool SupportsFragile { get; set; }
    public bool HasColdStorage { get; set; }
    public VehicleStatus VehicleStatus { get; set; }
    public DateTime? LastServiceDate { get; set; }
    public DateTime InsuranceExpiry { get; set; }
    public FuelType FuelType { get; set; }
    public long? CurrentBranchId { get; set; }
}
