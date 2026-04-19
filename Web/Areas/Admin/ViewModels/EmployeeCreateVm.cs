using System.ComponentModel;
using Base.Entities;
using Base.Enum;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.Areas.Admin.ViewModels;

public class EmployeeCreateVm
{
    [DisplayName("Employee Code")] public string EmployeeCode { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    [DisplayName("Alternate Phone")] public string? AlternatePhone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    [DisplayName("Employee Type")] public EmployeeType EmployeeType { get; set; } = EmployeeType.Office;
    [DisplayName("Employee Status")] public EmployeeStatus EmployeeStatus { get; set; } = EmployeeStatus.Active;
    [DisplayName("Joining Date")] public DateTime JoiningDate { get; set; } = DateTime.UtcNow;
    public string? Department { get; set; }
    public string? Designation { get; set; }

    [DisplayName("Branch")] public long CurrentBranchId { get; set; }
    public List<Branch> Branches { get; set; } = new();
    public SelectList BranchOptions => new(Branches, nameof(Branch.Id), nameof(Branch.Name), CurrentBranchId);

    // Optional User account creation
    [DisplayName("Create System Login")] public bool CreateUserAccount { get; set; }
    public string? Password { get; set; }
    [DisplayName("Confirm Password")] public string? ConfirmPassword { get; set; }

    // Driver fields
    [DisplayName("License Number")] public string? LicenseNumber { get; set; }
    [DisplayName("License Expiry")] public DateTime? LicenseExpiry { get; set; }
    public string? Username { get; set; }
}
