using Base.Enum.Consignment;

namespace Base.Dtos.Consignment;

public class ConsignmentUpdateDto
{
    public ServiceType ServiceType { get; set; }
    public PaymentMode PaymentMode { get; set; }
    public decimal? DeclaredValue { get; set; }
    public decimal? CodAmount { get; set; }
    public string? SpecialInstructions { get; set; }
    public long? DestinationBranchId { get; set; }
}
