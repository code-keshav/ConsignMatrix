using System.ComponentModel.DataAnnotations.Schema;
using Base.Entities.Interfaces;
using Base.Enum.Consignment;

namespace Base.Entities.Consignment;

[Table("trip", Schema = "consignment")]
public class Trip : BaseEntity, ISoftDelete
{
    public required string TripNumber { get; set; }
    public TripType TripType { get; set; }

    public long FromBranchId { get; set; }
    public virtual Branch FromBranch { get; set; }

    public long ToBranchId { get; set; }
    public virtual Branch ToBranch { get; set; }

    public long DriverId { get; set; }
    public virtual Employee Driver { get; set; }

    public long VehicleId { get; set; }
    public virtual Vehicle Vehicle { get; set; }

    public DateTime? ScheduledDeparture { get; set; }
    public DateTime? ActualDeparture { get; set; }

    public TripStatus TripStatus { get; set; } = TripStatus.Scheduled;

    public int TotalConsignments { get; set; }
    public decimal TotalWeight { get; set; }
    public string? Notes { get; set; }

    public virtual List<TripConsignment> TripConsignments { get; set; } = new();
}
