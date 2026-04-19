using System.ComponentModel;
using Base.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.Areas.Admin.ViewModels;

public class UserCreateVm
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    [DisplayName("Confirm Password")] public string ConfirmPassword { get; set; }
    [DisplayName("Contact No")] public string ContactNo { get; set; }
    public string? Address { get; set; }
    public List<Branch> Branches { get; set; } = new();
    public long BranchId { get; set; }
    public SelectList BranchOptions => new SelectList(Branches, nameof(Branch.Id), nameof(Branch.Name), BranchId);
    [DisplayName("Active")] public bool IsActive { get; set; } = true;
}