using Base.Enum.Consignment;
using Web.Areas.Consignment.ViewModels.Package;

namespace Web.Areas.Consignment.ViewModels.Consignment;

public class ConsignmentCreateVm
{
    // Sender
    public long? SenderId { get; set; }
    public long? SenderAddressId { get; set; }
    public string? SenderName { get; set; }
    public string? SenderPhone { get; set; }
    public string? SenderEmail { get; set; }
    public CustomerType? SenderCustomerType { get; set; }

    // New sender address
    public CustomerAddressVm? NewSenderAddress { get; set; }

    // Receiver
    public long? ReceiverId { get; set; }
    public long? ReceiverAddressId { get; set; }
    public string? ReceiverName { get; set; }
    public string? ReceiverPhone { get; set; }
    public string? ReceiverEmail { get; set; }
    public CustomerType? ReceiverCustomerType { get; set; }

    // New receiver address
    public CustomerAddressVm? NewReceiverAddress { get; set; }

    // Service
    public long DestinationBranchId { get; set; }
    public ServiceType ServiceType { get; set; }
    public PaymentMode PaymentMode { get; set; }
    public decimal? DeclaredValue { get; set; }
    public decimal? CodAmount { get; set; }
    public string? SpecialInstructions { get; set; }

    // Packages
    public List<PackageAddVm> Packages { get; set; } = new();
}
