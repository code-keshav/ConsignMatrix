using Base.Enum.Consignment;

namespace Base.Dtos.Consignment;

public class PickupTaskCreateDto
{
    public long ConsignmentId { get; set; }
    public DateTime PickupDate { get; set; }
    public PickupSlot PickupSlot { get; set; }
    public string PickupAddress { get; set; }
    public string ContactPhone { get; set; }
    public string? ContactName { get; set; }
}
