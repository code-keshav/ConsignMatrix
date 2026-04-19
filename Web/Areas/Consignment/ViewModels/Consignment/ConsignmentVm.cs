using Base.Enum.Consignment;

namespace Web.Areas.Consignment.ViewModels.Consignment;

public class ConsignmentVm
{
    public long Id { get; set; }
    public string TrackingNumber { get; set; }
    public ServiceType ServiceType { get; set; }
    public PaymentMode PaymentMode { get; set; }
    public decimal? DeclaredValue { get; set; }
    public decimal? CodAmount { get; set; }
    public string? SpecialInstructions { get; set; }
    public long DestinationBranchId { get; set; }
    public string? DestinationBranchName { get; set; }
    public string? ReceiverPinCode { get; set; }

    // Sender info
    public long? SenderId { get; set; }
    public string? SenderName { get; set; }
    public string? SenderPhone { get; set; }
    public long? SenderAddressId { get; set; }
    public string? SenderAddressDisplay { get; set; }

    // Receiver info
    public long? ReceiverId { get; set; }
    public string? ReceiverName { get; set; }
    public string? ReceiverPhone { get; set; }
    public long? ReceiverAddressId { get; set; }
    public string? ReceiverAddressDisplay { get; set; }
}
