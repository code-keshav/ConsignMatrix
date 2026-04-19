using Base.Enum.Consignment;

namespace Web.Areas.Consignment.ViewModels.Consignment;

public class ConsignmentStatusLogVm
{
    public ConsignmentStatusType StatusType { get; set; }
    public string? Remarks { get; set; }
    public DateTime RecDate { get; set; }
    public string? RecByName { get; set; }
}
