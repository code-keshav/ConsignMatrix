using Base.Enum.Consignment;
using Web.Areas.Consignment.ViewModels.Package;

namespace Web.Areas.Consignment.ViewModels.Consignment;

public class ConsignmentDetailVm
{
    public long Id { get; set; }
    public string TrackingNumber { get; set; }

    // Sender
    public string SenderName { get; set; }
    public string SenderPhone { get; set; }
    public string SenderAddress { get; set; }

    // Receiver
    public string ReceiverName { get; set; }
    public string ReceiverPhone { get; set; }
    public string ReceiverAddress { get; set; }

    // Branches
    public string OriginBranchName { get; set; }
    public string DestinationBranchName { get; set; }

    // Service
    public ServiceType ServiceType { get; set; }
    public PaymentMode PaymentMode { get; set; }
    public decimal? DeclaredValue { get; set; }
    public decimal? CodAmount { get; set; }
    public string? SpecialInstructions { get; set; }

    // Weight
    public decimal TotalWeight { get; set; }
    public decimal VolumetricWeight { get; set; }
    public decimal ChargeableWeight { get; set; }
    public decimal TotalVolume { get; set; }
    public int PackageCount { get; set; }

    // Dates
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    public DateTime RecDate { get; set; }

    // Status
    public string? LatestStatus { get; set; }
    public bool IsActive { get; set; }

    // Collections
    public List<PackageVm> Packages { get; set; } = new();
    public List<ConsignmentStatusLogVm> StatusLogs { get; set; } = new();
}
