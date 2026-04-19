namespace Web.Areas.Admin.Responses;

public class BranchPinCodeReportResponse
{
    public long Id { get; set; }
    public long BranchId { get; set; }
    public string BranchName { get; set; }
    public string PinCode { get; set; }
    public bool IsActive { get; set; }
}
