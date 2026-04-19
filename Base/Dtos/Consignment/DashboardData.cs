using Base.Enum;
using Base.Enum.Consignment;

namespace Base.Dtos.Consignment;

public class DashboardData
{
    // KPI Cards
    public long TotalActiveConsignments { get; set; }
    public long TodaysBookings { get; set; }
    public long PendingPickups { get; set; }
    public long InTransitCount { get; set; }
    public long OutForDeliveryCount { get; set; }
    public long DeliveredToday { get; set; }

    // Chart Data
    public Dictionary<ConsignmentStatusType, long> StatusDistribution { get; set; } = new();
    public List<DailyBookingCount> BookingTrend { get; set; } = new();
    public Dictionary<ServiceType, long> ServiceTypeDistribution { get; set; } = new();

    // Operations
    public PickupPerformanceData PickupPerformance { get; set; } = new();
    public Dictionary<TripStatus, long> TripStatusSummary { get; set; } = new();

    // Tables
    public List<RecentConsignmentItem> RecentConsignments { get; set; } = new();
    public List<UpcomingPickupItem> UpcomingPickups { get; set; } = new();
    public List<ActiveTripItem> ActiveTrips { get; set; } = new();
    public List<IssueItem> RecentIssues { get; set; } = new();

    // Branch & Fleet
    public List<BranchCapacityItem> BranchCapacities { get; set; } = new();
    public Dictionary<VehicleStatus, long> VehicleStatusSummary { get; set; } = new();
}

public class DailyBookingCount
{
    public DateTime Date { get; set; }
    public long Count { get; set; }
}

public class PickupPerformanceData
{
    public long Pending { get; set; }
    public long Completed { get; set; }
    public long Failed { get; set; }
    public long Cancelled { get; set; }
    public long ScheduledToday { get; set; }
    public long CompletedToday { get; set; }
}

public class RecentConsignmentItem
{
    public long Id { get; set; }
    public string TrackingNumber { get; set; } = "";
    public string SenderName { get; set; } = "";
    public string ReceiverName { get; set; } = "";
    public ServiceType ServiceType { get; set; }
    public ConsignmentStatusType? LatestStatus { get; set; }
    public DateTime RecDate { get; set; }
}

public class UpcomingPickupItem
{
    public long Id { get; set; }
    public string TrackingNumber { get; set; } = "";
    public DateTime PickupDate { get; set; }
    public PickupSlot PickupSlot { get; set; }
    public PickupTaskStatus TaskStatus { get; set; }
    public string? AssignedDriverName { get; set; }
}

public class ActiveTripItem
{
    public long Id { get; set; }
    public string TripNumber { get; set; } = "";
    public TripType TripType { get; set; }
    public TripStatus TripStatus { get; set; }
    public string FromBranchName { get; set; } = "";
    public string ToBranchName { get; set; } = "";
    public int TotalConsignments { get; set; }
    public DateTime? ScheduledDeparture { get; set; }
    public string DriverName { get; set; } = "";
}

public class IssueItem
{
    public long ConsignmentId { get; set; }
    public string TrackingNumber { get; set; } = "";
    public ConsignmentStatusType StatusType { get; set; }
    public string? Remarks { get; set; }
    public DateTime OccurredAt { get; set; }
}

public class BranchCapacityItem
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public BranchType BranchType { get; set; }
    public decimal StorageCapacity { get; set; }
    public decimal CurrentLoad { get; set; }
    public int UtilizationPercent { get; set; }
}
