namespace Web.Areas.Consignment.ViewModels.Pickup;

public class PickupCompleteVm
{
    public long PickupTaskId { get; set; }
    public decimal VerifiedWeight { get; set; }
    public string? Remarks { get; set; }
}
