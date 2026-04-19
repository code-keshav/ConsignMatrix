using Base.Dtos.Consignment;
using Base.Enum;
using Base.Enum.Consignment;
using Base.Providers.Interfaces;
using Base.Services.Consignment.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Areas.Admin.ViewModels;
using Web.Helpers;

namespace Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class HomeController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public HomeController(IDashboardService dashboardService, ICurrentUserProvider currentUserProvider)
    {
        _dashboardService = dashboardService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int daysBack = 30)
    {
        // Clamp daysBack to valid presets
        if (daysBack != 1 && daysBack != 7 && daysBack != 30)
            daysBack = 30;

        var isMainBranch = await _currentUserProvider.IsMainBranch();
        long? branchFilter = isMainBranch ? null : await _currentUserProvider.GetUserBranchId();

        var data = await _dashboardService.GetDashboardDataAsync(branchFilter, daysBack);

        var viewModel = MapToViewModel(data, isMainBranch, daysBack);
        return View(viewModel);
    }

    [HttpGet]
    [Route("/Account/AccessDenied")]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private static AdminDashboardVm MapToViewModel(DashboardData data, bool isMainBranch, int daysBack)
    {
        var vm = new AdminDashboardVm
        {
            // KPI Cards
            TotalActiveConsignments = data.TotalActiveConsignments,
            TodaysBookings = data.TodaysBookings,
            PendingPickups = data.PendingPickups,
            InTransitCount = data.InTransitCount,
            OutForDeliveryCount = data.OutForDeliveryCount,
            DeliveredToday = data.DeliveredToday,

            // Pickup Performance
            PickupsPending = data.PickupPerformance.Pending,
            PickupsCompleted = data.PickupPerformance.Completed,
            PickupsFailed = data.PickupPerformance.Failed,
            PickupsCancelled = data.PickupPerformance.Cancelled,
            PickupsScheduledToday = data.PickupPerformance.ScheduledToday,
            PickupsCompletedToday = data.PickupPerformance.CompletedToday,

            // Trip Summary
            TripsScheduled = data.TripStatusSummary.GetValueOrDefault(TripStatus.Scheduled),
            TripsInTransit = data.TripStatusSummary.GetValueOrDefault(TripStatus.InTransit),
            TripsCompleted = data.TripStatusSummary.GetValueOrDefault(TripStatus.Completed),
            TripsCancelled = data.TripStatusSummary.GetValueOrDefault(TripStatus.Cancelled),

            // Vehicle Status
            VehiclesAvailable = data.VehicleStatusSummary.GetValueOrDefault(VehicleStatus.Available),
            VehiclesOnTrip = data.VehicleStatusSummary.GetValueOrDefault(VehicleStatus.OnTrip),
            VehiclesMaintenance = data.VehicleStatusSummary.GetValueOrDefault(VehicleStatus.Maintenance),
            VehiclesInactive = data.VehicleStatusSummary.GetValueOrDefault(VehicleStatus.Inactive),

            // Filter State
            SelectedDaysBack = daysBack,
            IsMainBranchUser = isMainBranch
        };

        // Status Distribution Chart
        vm.StatusDistribution = data.StatusDistribution
            .OrderBy(kvp => (int)kvp.Key)
            .Select(kvp => new ChartItem
            {
                Label = DashboardHelper.GetStatusDisplayName(kvp.Key),
                Value = kvp.Value,
                Color = DashboardHelper.GetStatusChartColor(kvp.Key)
            })
            .ToList();

        // Booking Trend Chart
        vm.BookingTrend = data.BookingTrend
            .Select(b => new TrendChartItem
            {
                Date = b.Date.ToString("MMM dd"),
                Count = b.Count
            })
            .ToList();

        // Service Type Distribution Chart
        var serviceTypeColors = new Dictionary<ServiceType, string>
        {
            { ServiceType.Standard, "#0d6efd" },
            { ServiceType.Express, "#fd7e14" },
            { ServiceType.SameDay, "#dc3545" },
            { ServiceType.Scheduled, "#6f42c1" }
        };
        vm.ServiceTypeDistribution = data.ServiceTypeDistribution
            .Select(kvp => new ChartItem
            {
                Label = kvp.Key.ToString(),
                Value = kvp.Value,
                Color = serviceTypeColors.GetValueOrDefault(kvp.Key, "#adb5bd")
            })
            .ToList();

        // Vehicle Status Chart
        var vehicleStatusColors = new Dictionary<VehicleStatus, string>
        {
            { VehicleStatus.Available, "#198754" },
            { VehicleStatus.OnTrip, "#0d6efd" },
            { VehicleStatus.Maintenance, "#ffc107" },
            { VehicleStatus.Inactive, "#6c757d" }
        };
        vm.VehicleStatusChart = data.VehicleStatusSummary
            .Select(kvp => new ChartItem
            {
                Label = kvp.Key == VehicleStatus.OnTrip ? "On Trip" : kvp.Key.ToString(),
                Value = kvp.Value,
                Color = vehicleStatusColors.GetValueOrDefault(kvp.Key, "#adb5bd")
            })
            .ToList();

        // Recent Consignments Table
        vm.RecentConsignments = data.RecentConsignments
            .Select(c => new RecentConsignmentRow
            {
                Id = c.Id,
                TrackingNumber = c.TrackingNumber,
                SenderName = c.SenderName,
                ReceiverName = c.ReceiverName,
                ServiceType = c.ServiceType.ToString(),
                LatestStatus = c.LatestStatus.HasValue
                    ? DashboardHelper.GetStatusDisplayName(c.LatestStatus.Value)
                    : "N/A",
                StatusBadgeClass = DashboardHelper.GetStatusBadgeClass(c.LatestStatus),
                RecDate = c.RecDate.ToString("yyyy-MM-dd")
            })
            .ToList();

        // Upcoming Pickups Table
        vm.UpcomingPickups = data.UpcomingPickups
            .Select(p => new UpcomingPickupRow
            {
                Id = p.Id,
                TrackingNumber = p.TrackingNumber,
                PickupDate = p.PickupDate.ToString("yyyy-MM-dd"),
                PickupSlot = p.PickupSlot.ToString(),
                TaskStatus = p.TaskStatus.ToString(),
                StatusBadgeClass = DashboardHelper.GetPickupStatusBadgeClass(p.TaskStatus),
                AssignedDriverName = p.AssignedDriverName ?? "Unassigned"
            })
            .ToList();

        // Active Trips Table
        vm.ActiveTrips = data.ActiveTrips
            .Select(t => new ActiveTripRow
            {
                Id = t.Id,
                TripNumber = t.TripNumber,
                TripType = t.TripType.ToString(),
                TripStatus = t.TripStatus.ToString(),
                TripStatusBadgeClass = DashboardHelper.GetTripStatusBadgeClass(t.TripStatus),
                FromBranch = t.FromBranchName,
                ToBranch = t.ToBranchName,
                ConsignmentCount = t.TotalConsignments,
                DriverName = t.DriverName
            })
            .ToList();

        // Recent Issues Table
        vm.RecentIssues = data.RecentIssues
            .Select(i => new IssueRow
            {
                ConsignmentId = i.ConsignmentId,
                TrackingNumber = i.TrackingNumber,
                IssueType = DashboardHelper.GetStatusDisplayName(i.StatusType),
                IssueBadgeClass = DashboardHelper.GetIssueBadgeClass(i.StatusType),
                Remarks = i.Remarks ?? "",
                OccurredAt = i.OccurredAt.ToString("yyyy-MM-dd HH:mm")
            })
            .ToList();

        // Branch Capacities
        vm.BranchCapacities = data.BranchCapacities
            .Select(b => new BranchCapacityRow
            {
                Id = b.Id,
                Name = b.Name,
                Code = b.Code,
                BranchType = b.BranchType.ToString(),
                StorageCapacity = b.StorageCapacity,
                CurrentLoad = b.CurrentLoad,
                UtilizationPercent = b.UtilizationPercent,
                BarColorClass = DashboardHelper.GetCapacityBarClass(b.UtilizationPercent)
            })
            .ToList();

        return vm;
    }
}
