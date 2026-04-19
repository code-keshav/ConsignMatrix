using Base.Enum.Consignment;

namespace Web.Areas.Admin.ViewModels;

public class AdminDashboardVm
{
    // KPI Cards
    public long TotalActiveConsignments { get; set; }
    public long TodaysBookings { get; set; }
    public long PendingPickups { get; set; }
    public long InTransitCount { get; set; }
    public long OutForDeliveryCount { get; set; }
    public long DeliveredToday { get; set; }

    // Chart Data
    public List<ChartItem> StatusDistribution { get; set; } = new();
    public List<TrendChartItem> BookingTrend { get; set; } = new();
    public List<ChartItem> ServiceTypeDistribution { get; set; } = new();

    // Pickup Performance
    public long PickupsPending { get; set; }
    public long PickupsCompleted { get; set; }
    public long PickupsFailed { get; set; }
    public long PickupsCancelled { get; set; }
    public long PickupsScheduledToday { get; set; }
    public long PickupsCompletedToday { get; set; }

    // Trip Summary
    public long TripsScheduled { get; set; }
    public long TripsInTransit { get; set; }
    public long TripsCompleted { get; set; }
    public long TripsCancelled { get; set; }

    // Tables
    public List<RecentConsignmentRow> RecentConsignments { get; set; } = new();
    public List<UpcomingPickupRow> UpcomingPickups { get; set; } = new();
    public List<ActiveTripRow> ActiveTrips { get; set; } = new();
    public List<IssueRow> RecentIssues { get; set; } = new();

    // Branch & Fleet
    public List<BranchCapacityRow> BranchCapacities { get; set; } = new();
    public long VehiclesAvailable { get; set; }
    public long VehiclesOnTrip { get; set; }
    public long VehiclesMaintenance { get; set; }
    public long VehiclesInactive { get; set; }
    public List<ChartItem> VehicleStatusChart { get; set; } = new();

    // Filter State
    public int SelectedDaysBack { get; set; } = 30;
    public bool IsMainBranchUser { get; set; }
}

public class ChartItem
{
    public string Label { get; set; } = "";
    public long Value { get; set; }
    public string Color { get; set; } = "";
}

public class TrendChartItem
{
    public string Date { get; set; } = "";
    public long Count { get; set; }
}

public class RecentConsignmentRow
{
    public long Id { get; set; }
    public string TrackingNumber { get; set; } = "";
    public string SenderName { get; set; } = "";
    public string ReceiverName { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public string LatestStatus { get; set; } = "";
    public string StatusBadgeClass { get; set; } = "";
    public string RecDate { get; set; } = "";
}

public class UpcomingPickupRow
{
    public long Id { get; set; }
    public string TrackingNumber { get; set; } = "";
    public string PickupDate { get; set; } = "";
    public string PickupSlot { get; set; } = "";
    public string TaskStatus { get; set; } = "";
    public string StatusBadgeClass { get; set; } = "";
    public string AssignedDriverName { get; set; } = "";
}

public class ActiveTripRow
{
    public long Id { get; set; }
    public string TripNumber { get; set; } = "";
    public string TripType { get; set; } = "";
    public string TripStatus { get; set; } = "";
    public string TripStatusBadgeClass { get; set; } = "";
    public string FromBranch { get; set; } = "";
    public string ToBranch { get; set; } = "";
    public int ConsignmentCount { get; set; }
    public string DriverName { get; set; } = "";
}

public class IssueRow
{
    public long ConsignmentId { get; set; }
    public string TrackingNumber { get; set; } = "";
    public string IssueType { get; set; } = "";
    public string IssueBadgeClass { get; set; } = "";
    public string Remarks { get; set; } = "";
    public string OccurredAt { get; set; } = "";
}

public class BranchCapacityRow
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public string BranchType { get; set; } = "";
    public decimal StorageCapacity { get; set; }
    public decimal CurrentLoad { get; set; }
    public int UtilizationPercent { get; set; }
    public string BarColorClass { get; set; } = "";
}
