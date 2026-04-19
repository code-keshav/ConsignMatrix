using System.ComponentModel.DataAnnotations.Schema;
using Base.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.Areas.Admin.ViewModels;

public class UserTransferRequest
{
    public long UserId { get; set; }
    public long FromBranchId { get; set; }
    public long ToBranchId { get; set; }
    public string Note { get; set; }
    public List<User> Users { get; set; } = new List<User>();
    public List<Branch> ToBranches { get; set; } = new List<Branch>();

    public SelectList UserSelectList => new(Users.Select(x => new {x.Id, Name = $"{x.Name} ({x.Branch.Name})"}), "Id", "Name");
    public SelectList ToBranchSelectList => new(ToBranches, nameof(Branch.Id), nameof(Branch.Name));
    public bool HasPermission { get; set; }
    
    [NotMapped]
    [ValidateNever]
    public Branch? FromBranch { get; set; }
}