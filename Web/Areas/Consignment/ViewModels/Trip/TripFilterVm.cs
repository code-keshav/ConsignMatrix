using Base.Enum.Consignment;

namespace Web.Areas.Consignment.ViewModels.Trip;

public enum TripDirection
{
    Outgoing = 1,
    Incoming = 2,
    All = 3
}

public class TripFilterVm
{
    public TripStatus? Status { get; set; }
    public TripType? TripType { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? TripNumber { get; set; }
    public TripDirection Direction { get; set; } = TripDirection.Outgoing;
    public long CurrentBranchId { get; set; }

    public List<TripListItemVm> Trips { get; set; } = new();
}
