using System.ComponentModel;
using Base.Enum;

namespace Web.Areas.Admin.ViewModels;

public class BranchEditVm
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    [DisplayName("Branch Type")] public BranchType BranchType { get; set; } = BranchType.ServiceCenter;
    public string Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    [DisplayName("Contact No")] public string ContactNo { get; set; }
    public string? Email { get; set; }
    [DisplayName("Storage Capacity")] public decimal? StorageCapacity { get; set; }
    [DisplayName("Operating Hours")] public string? OperatingHours { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}
