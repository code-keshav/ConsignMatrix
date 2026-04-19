using System.ComponentModel.DataAnnotations.Schema;
using Base.Entities.Interfaces;
using Base.Enum.Consignment;

namespace Base.Entities.Consignment;

[Table("consignment", Schema = "consignment")]
public class Consignment : BaseEntity, ISoftDelete
{
    public required string TrackingNumber { get; set; }

    public long SenderId { get; set; }
    public virtual Customer Sender { get; set; }
    public long SenderAddressId { get; set; }
    public virtual CustomerAddress SenderAddress { get; set; }

    public long ReceiverId { get; set; }
    public virtual Customer Receiver { get; set; }
    public long ReceiverAddressId { get; set; }
    public virtual CustomerAddress ReceiverAddress { get; set; }

    public long OriginBranchId { get; set; }
    public virtual Branch OriginBranch { get; set; }
    public long DestinationBranchId { get; set; }
    public virtual Branch DestinationBranch { get; set; }

    public ServiceType ServiceType { get; set; }
    public PaymentMode PaymentMode { get; set; }
    public decimal? DeclaredValue { get; set; }
    public decimal? CodAmount { get; set; }
    public string? SpecialInstructions { get; set; }

    public decimal TotalWeight { get; set; }
    public decimal VolumetricWeight { get; set; }
    public decimal ChargeableWeight { get; set; }
    public decimal TotalVolume { get; set; }
    public int PackageCount { get; set; }

    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }

    public virtual List<Package> Packages { get; set; }
    public virtual List<ConsignmentStatusLog> StatusLogs { get; set; }
    public virtual List<PickupTask> PickupTasks { get; set; }
}
