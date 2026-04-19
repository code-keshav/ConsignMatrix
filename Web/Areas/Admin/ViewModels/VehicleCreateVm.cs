using System.ComponentModel;
using Base.Entities;
using Base.Enum;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.Areas.Admin.ViewModels;

public class VehicleCreateVm
{
    [DisplayName("Vehicle Number")] public string VehicleNumber { get; set; }
    [DisplayName("Vehicle Type")] public VehicleType VehicleType { get; set; }
    [DisplayName("Ownership Type")] public VehicleOwnershipType OwnershipType { get; set; }
    [DisplayName("Max Weight Capacity (kg)")] public decimal MaxWeightCapacity { get; set; }
    [DisplayName("Max Volume Capacity (m³)")] public decimal MaxVolumeCapacity { get; set; }
    [DisplayName("Supports Fragile")] public bool SupportsFragile { get; set; }
    [DisplayName("Has Cold Storage")] public bool HasColdStorage { get; set; }
    [DisplayName("Vehicle Status")] public VehicleStatus VehicleStatus { get; set; } = VehicleStatus.Available;
    [DisplayName("Last Service Date")] public DateTime? LastServiceDate { get; set; }
    [DisplayName("Insurance Expiry")] public DateTime InsuranceExpiry { get; set; } = DateTime.Today;
    [DisplayName("Fuel Type")] public FuelType FuelType { get; set; }

    [DisplayName("Branch")] public long? CurrentBranchId { get; set; }
    public List<Branch> Branches { get; set; } = new();
    public SelectList BranchOptions => new(Branches, nameof(Branch.Id), nameof(Branch.Name), CurrentBranchId);
}
