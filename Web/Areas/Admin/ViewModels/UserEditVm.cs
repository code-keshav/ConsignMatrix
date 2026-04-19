using System.ComponentModel;

namespace Web.Areas.Admin.ViewModels;

public class UserEditVm
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    [DisplayName("Contact No")] public string ContactNo { get; set; }
    public string? Address { get; set; }
    [DisplayName("Active")] public bool IsActive { get; set; }
}