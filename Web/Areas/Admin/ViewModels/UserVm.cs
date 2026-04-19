using Base.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.Areas.Admin.ViewModels;

public class UserVm
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string ContactNo { get; set; }
    public string? Address { get; set; }
    public string BranchName { get; set; }
    public string BranchCode { get; set; }
    public string UserLevelDisplay { get; set; }
    public string? Role => string.Join(", ", Roles.Select(x => x.Name));
    public List<long> RoleIds => Roles.Select(x => x.Id).ToList();

    public IEnumerable<Role> Roles { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsSuperAdmin { get; set; }
    public bool IsMainBranchUser { get; set; }
    public bool HasPermission { get; set; }
    public bool IsActive { get; set; }
}

public class UserWithRolesVm
{
    public List<UserVm> Users { get; set; }
    public List<Role> Roles { get; set; }
    public List<Branch> Branches { get; set; } = new();

    // Filter properties
    public string? SearchTerm { get; set; }
    public long? BranchId { get; set; }
    public long? RoleId { get; set; }
    public int? UserLevel { get; set; }

    // Pagination properties
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    // Current user info (for showing/hiding branch filter)
    public bool IsMainBranchUser { get; set; }

    public MultiSelectList RoleSelectList => new(Roles, nameof(Role.Id), nameof(Role.Name));
    public SelectList BranchSelectList => new(Branches, nameof(Branch.Id), nameof(Branch.Name));
    public SelectList UserLevelSelectList => new SelectList(new[]
    {
        new { Value = 1, Text = "SuperAdmin" },
        new { Value = 2, Text = "Admin" },
        new { Value = 3, Text = "BranchAdmin" },
        new { Value = 4, Text = "User" }
    }, "Value", "Text");
}