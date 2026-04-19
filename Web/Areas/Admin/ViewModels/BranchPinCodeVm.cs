namespace Web.Areas.Admin.ViewModels;

public class BranchPinCodeVm
{
    public long Id { get; set; }
    public long BranchId { get; set; }
    public string PinCode { get; set; }
    public bool IsActive { get; set; } = true;
}
