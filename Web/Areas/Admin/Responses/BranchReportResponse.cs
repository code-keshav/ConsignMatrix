using Base.Entities;
using Base.Enum;

namespace Web.Areas.Admin.Responses;

public class BranchReportResponse
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public BranchType BranchType { get; set; }
    public string Address { get; set; }
    public string? City { get; set; }
    public string ContactNo { get; set; }
    public string? Email { get; set; }
    public StatusEnum Status { get; set; }
    public int UserCount { get; set; }
    public int PinCodeCount { get; set; }
}
