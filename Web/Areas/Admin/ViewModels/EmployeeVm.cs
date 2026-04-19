using Base.Entities;
using Base.Enum;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.Areas.Admin.ViewModels;

public class EmployeeListItemVm
{
    public long Id { get; set; }
    public string EmployeeCode { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public string? Email { get; set; }
    public EmployeeType EmployeeType { get; set; }
    public EmployeeStatus EmployeeStatus { get; set; }
    public string BranchName { get; set; }
    public string BranchCode { get; set; }
    public bool HasLogin { get; set; }
    public string? Department { get; set; }
    public string? Designation { get; set; }
    public long? DriverId { get; set; }
}

public class EmployeeIndexVm
{
    public List<EmployeeListItemVm> Employees { get; set; } = new();
    public List<Branch> Branches { get; set; } = new();

    // Filter properties
    public string? SearchTerm { get; set; }
    public long? BranchId { get; set; }
    public int? EmployeeType { get; set; }
    public int? EmployeeStatus { get; set; }

    // Pagination
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public bool IsMainBranchUser { get; set; }

    public SelectList BranchSelectList => new(Branches, nameof(Branch.Id), nameof(Branch.Name));

    public SelectList EmployeeTypeSelectList => new(new[]
    {
        new { Value = 1, Text = "Office" },
        new { Value = 2, Text = "Driver" },
        new { Value = 3, Text = "FieldWorker" },
        new { Value = 4, Text = "Support" }
    }, "Value", "Text");

    public SelectList EmployeeStatusSelectList => new(new[]
    {
        new { Value = 1, Text = "Active" },
        new { Value = 2, Text = "On Leave" },
        new { Value = 3, Text = "Inactive" },
        new { Value = 4, Text = "Terminated" }
    }, "Value", "Text");
}
