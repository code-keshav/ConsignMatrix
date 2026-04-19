using Base.Enum.Consignment;

namespace Base.Dtos.Consignment;

public class ConsignmentDto
{
    public long? SenderId { get; set; }
    public long? SenderAddressId { get; set; }
    public CustomerDto? NewSender { get; set; }
    public CustomerAddressDto? NewSenderAddress { get; set; }

    public long? ReceiverId { get; set; }
    public long? ReceiverAddressId { get; set; }
    public CustomerDto? NewReceiver { get; set; }
    public CustomerAddressDto? NewReceiverAddress { get; set; }

    public long DestinationBranchId { get; set; }

    public ServiceType ServiceType { get; set; }
    public PaymentMode PaymentMode { get; set; }
    public decimal? DeclaredValue { get; set; }
    public decimal? CodAmount { get; set; }
    public string? SpecialInstructions { get; set; }

    public List<PackageDto> Packages { get; set; } = new();
    public long BranchId { get; set; }
}
