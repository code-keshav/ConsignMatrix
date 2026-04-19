using Base.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.Areas.Admin.ViewModels;

public class DriverListItemVm
{
    public long Id { get; set; }
    public long EmployeeId { get; set; }
    public string EmployeeCode { get; set; }
    public string EmployeeName { get; set; }
    public string BranchName { get; set; }
    public string BranchCode { get; set; }
    public string Phone { get; set; }
    public string LicenseNumber { get; set; }
    public DateTime LicenseExpiry { get; set; }
}

public class DriverIndexVm
{
    public List<DriverListItemVm> Drivers { get; set; } = new();
    public List<Branch> Branches { get; set; } = new();

    // Filter properties
    public string? SearchTerm { get; set; }
    public long? BranchId { get; set; }
    public bool LicenseExpiringSoon { get; set; }

    // Pagination
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public bool IsMainBranchUser { get; set; }

    public SelectList BranchSelectList => new(Branches, nameof(Branch.Id), nameof(Branch.Name));
}
