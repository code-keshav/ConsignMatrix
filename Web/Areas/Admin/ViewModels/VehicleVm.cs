using Base.Entities;
using Base.Enum;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.Areas.Admin.ViewModels;

public class VehicleListItemVm
{
    public long Id { get; set; }
    public string VehicleNumber { get; set; }
    public VehicleType VehicleType { get; set; }
    public VehicleOwnershipType OwnershipType { get; set; }
    public decimal MaxWeightCapacity { get; set; }
    public decimal MaxVolumeCapacity { get; set; }
    public VehicleStatus VehicleStatus { get; set; }
    public DateTime InsuranceExpiry { get; set; }
    public FuelType FuelType { get; set; }
    public string? BranchName { get; set; }
    public string? BranchCode { get; set; }
}

public class VehicleIndexVm
{
    public List<VehicleListItemVm> Vehicles { get; set; } = new();
    public List<Branch> Branches { get; set; } = new();

    // Filter properties
    public string? SearchTerm { get; set; }
    public long? BranchId { get; set; }
    public int? VehicleType { get; set; }
    public int? VehicleStatus { get; set; }

    // Pagination
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public bool IsMainBranchUser { get; set; }

    public SelectList BranchSelectList => new(Branches, nameof(Branch.Id), nameof(Branch.Name));

    public SelectList VehicleTypeSelectList => new(new[]
    {
        new { Value = 1, Text = "Bike" },
        new { Value = 2, Text = "Van" },
        new { Value = 3, Text = "Truck" },
        new { Value = 4, Text = "Container" }
    }, "Value", "Text");

    public SelectList VehicleStatusSelectList => new(new[]
    {
        new { Value = 1, Text = "Available" },
        new { Value = 2, Text = "On Trip" },
        new { Value = 3, Text = "Maintenance" },
        new { Value = 4, Text = "Inactive" }
    }, "Value", "Text");
}
