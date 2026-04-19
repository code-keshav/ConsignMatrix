namespace Base.Dtos.Consignment;

public class PickupCompleteDto
{
    public long PickupTaskId { get; set; }
    public decimal VerifiedWeight { get; set; }
    public string? Remarks { get; set; }
}
