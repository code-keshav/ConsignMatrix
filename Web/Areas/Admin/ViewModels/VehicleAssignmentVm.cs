using Base.Entities;
using Base.Enum;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.Areas.Admin.ViewModels;

public class VehicleAssignmentListItemVm
{
    public long Id { get; set; }
    public string VehicleNumber { get; set; }
    public long VehicleId { get; set; }
    public string EmployeeName { get; set; }
    public string EmployeeCode { get; set; }
    public VehicleAssignmentType AssignmentType { get; set; }
    public DateTime AssignedFrom { get; set; }
    public DateTime? AssignedTo { get; set; }
    public bool IsActive { get; set; }
}

public class VehicleAssignmentIndexVm
{
    public List<VehicleAssignmentListItemVm> Assignments { get; set; } = new();
    public List<Vehicle> Vehicles { get; set; } = new();

    // Filters
    public long? VehicleId { get; set; }
    public long? EmployeeId { get; set; }
    public int? AssignmentType { get; set; }
    public bool? IsActive { get; set; } = true;

    // Pagination
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public SelectList VehicleSelectList => new(Vehicles, nameof(Vehicle.Id), nameof(Vehicle.VehicleNumber));

    public SelectList AssignmentTypeSelectList => new(new[]
    {
        new { Value = 1, Text = "Driver" },
        new { Value = 2, Text = "Supporting Worker" },
        new { Value = 3, Text = "Helper" }
    }, "Value", "Text");

    public SelectList IsActiveSelectList => new(new[]
    {
        new { Value = "true", Text = "Active" },
        new { Value = "false", Text = "Inactive" }
    }, "Value", "Text");
}

public class VehicleAssignmentCreateVm
{
    public long? VehicleId { get; set; }
    public string? VehicleNumber { get; set; }
    public bool IsVehicleReadonly { get; set; }
    public List<VehicleAssignmentRowVm> Rows { get; set; } = new();

    // Dropdown data
    public List<Vehicle> Vehicles { get; set; } = new();
    public SelectList VehicleSelectList => new(Vehicles, nameof(Vehicle.Id), nameof(Vehicle.VehicleNumber));
}

public class VehicleAssignmentRowVm
{
    public long EmployeeId { get; set; }
    public VehicleAssignmentType AssignmentType { get; set; } = VehicleAssignmentType.Helper;
    public DateTime AssignedFrom { get; set; } = DateTime.UtcNow.Date;
    public DateTime? AssignedTo { get; set; }
}

public class VehicleAssignmentEditVm
{
    public long VehicleId { get; set; }
    public string VehicleNumber { get; set; }
    public List<VehicleAssignmentEditRowVm> Rows { get; set; } = new();
}

public class VehicleAssignmentEditRowVm
{
    public long Id { get; set; }
    public long EmployeeId { get; set; }
    public string EmployeeName { get; set; }
    public string EmployeeCode { get; set; }
    public VehicleAssignmentType AssignmentType { get; set; }
    public DateTime AssignedFrom { get; set; }
    public DateTime? AssignedTo { get; set; }
    public bool IsActive { get; set; }
}
