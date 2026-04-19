using System.ComponentModel;
using Base.Entities;
using Base.Enum;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.Areas.Admin.ViewModels;

public class EmployeeEditVm
{
    public long Id { get; set; }
    [DisplayName("Employee Code")] public string EmployeeCode { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    [DisplayName("Alternate Phone")] public string? AlternatePhone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    [DisplayName("Employee Type")] public EmployeeType EmployeeType { get; set; }
    [DisplayName("Employee Status")] public EmployeeStatus EmployeeStatus { get; set; }
    [DisplayName("Joining Date")] public DateTime JoiningDate { get; set; }
    [DisplayName("Termination Date")] public DateTime? TerminationDate { get; set; }
    public string? Department { get; set; }
    public string? Designation { get; set; }

    [DisplayName("Branch")] public long CurrentBranchId { get; set; }
    public List<Branch> Branches { get; set; } = new();
    public SelectList BranchOptions => new(Branches, nameof(Branch.Id), nameof(Branch.Name), CurrentBranchId);

    public bool HasUserAccount { get; set; }

    // Driver fields
    [DisplayName("License Number")] public string? LicenseNumber { get; set; }
    [DisplayName("License Expiry")] public DateTime? LicenseExpiry { get; set; }
    public long? DriverId { get; set; }
}
