using Base.Dtos.Consignment;
using Base.Entities;
using Base.Enum;
using Base.Enum.Consignment;
using Base.Repo.Consignment.Interfaces;
using Base.Repo.Interfaces;
using Base.Services.Consignment.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Base.Services.Consignment;

public class DashboardService : IDashboardService
{
    private readonly IConsignmentRepo _consignmentRepo;
    private readonly IConsignmentStatusLogRepo _statusLogRepo;
    private readonly IPickupTaskRepo _pickupTaskRepo;
    private readonly ITripRepo _tripRepo;
    private readonly IBranchRepo _branchRepo;
    private readonly IVehicleRepo _vehicleRepo;

    private static readonly HashSet<ConsignmentStatusType> TerminalStatuses = new()
    {
        ConsignmentStatusType.Delivered,
        ConsignmentStatusType.ReturnedToSender,
        ConsignmentStatusType.Damaged,
        ConsignmentStatusType.Lost
    };

    private static readonly HashSet<ConsignmentStatusType> IssueStatuses = new()
    {
        ConsignmentStatusType.DeliveryAttempted,
        ConsignmentStatusType.PickupAttempted,
        ConsignmentStatusType.Damaged,
        ConsignmentStatusType.Lost,
        ConsignmentStatusType.HeldAtBranch,
        ConsignmentStatusType.RtsInitiated
    };

    public DashboardService(
        IConsignmentRepo consignmentRepo,
        IConsignmentStatusLogRepo statusLogRepo,
        IPickupTaskRepo pickupTaskRepo,
        ITripRepo tripRepo,
        IBranchRepo branchRepo,
        IVehicleRepo vehicleRepo)
    {
        _consignmentRepo = consignmentRepo;
        _statusLogRepo = statusLogRepo;
        _pickupTaskRepo = pickupTaskRepo;
        _tripRepo = tripRepo;
        _branchRepo = branchRepo;
        _vehicleRepo = vehicleRepo;
    }

    public async Task<DashboardData> GetDashboardDataAsync(long? branchId, int daysBack = 30)
    {
        var todayStart = DateTime.UtcNow.Date;
        var tomorrowStart = todayStart.AddDays(1);
        var periodStart = todayStart.AddDays(-daysBack);

        var data = new DashboardData();

        // Build base consignment queryable with branch filter
        var consignmentQuery = _consignmentRepo.GetQueryable();
        if (branchId.HasValue)
            consignmentQuery = consignmentQuery.Where(c =>
                c.OriginBranchId == branchId.Value || c.DestinationBranchId == branchId.Value);

        // Get latest status per consignment (materialized once, reused for KPIs + status distribution)
        var consignmentIds = consignmentQuery.Select(c => c.Id);
        var latestStatusPerConsignment = await _statusLogRepo.GetQueryable()
            .Where(sl => consignmentIds.Contains(sl.ConsignmentId))
            .GroupBy(sl => sl.ConsignmentId)
            .Select(g => new
            {
                ConsignmentId = g.Key,
                LatestStatus = g.OrderByDescending(sl => sl.RecDate).First().StatusType
            })
            .ToListAsync();

        // KPI Cards
        data.TotalActiveConsignments = latestStatusPerConsignment
            .Count(x => !TerminalStatuses.Contains(x.LatestStatus));
        data.InTransitCount = latestStatusPerConsignment
            .Count(x => x.LatestStatus == ConsignmentStatusType.InTransit);
        data.OutForDeliveryCount = latestStatusPerConsignment
            .Count(x => x.LatestStatus == ConsignmentStatusType.OutForDelivery);

        data.TodaysBookings = await consignmentQuery
            .CountAsync(c => c.RecDate >= todayStart && c.RecDate < tomorrowStart);

        data.DeliveredToday = await _statusLogRepo.GetQueryable()
            .Where(sl => consignmentIds.Contains(sl.ConsignmentId))
            .CountAsync(sl => sl.StatusType == ConsignmentStatusType.Delivered
                              && sl.RecDate >= todayStart && sl.RecDate < tomorrowStart);

        // Status Distribution
        data.StatusDistribution = latestStatusPerConsignment
            .GroupBy(x => x.LatestStatus)
            .ToDictionary(g => g.Key, g => (long)g.Count());

        // Booking Trend
        data.BookingTrend = await consignmentQuery
            .Where(c => c.RecDate >= periodStart)
            .GroupBy(c => c.RecDate.Date)
            .Select(g => new DailyBookingCount { Date = g.Key, Count = g.LongCount() })
            .OrderBy(x => x.Date)
            .ToListAsync();

        // Service Type Distribution
        data.ServiceTypeDistribution = await consignmentQuery
            .GroupBy(c => c.ServiceType)
            .Select(g => new { Type = g.Key, Count = g.LongCount() })
            .ToDictionaryAsync(x => x.Type, x => x.Count);

        // Pickup Performance
        await LoadPickupPerformance(data, branchId, consignmentIds, todayStart, tomorrowStart);

        // Trip Status Summary
        await LoadTripStatusSummary(data, branchId);

        // Recent Consignments
        await LoadRecentConsignments(data, consignmentQuery);

        // Upcoming Pickups
        await LoadUpcomingPickups(data, consignmentIds, todayStart);

        // Active Trips
        await LoadActiveTrips(data, branchId);

        // Recent Issues
        await LoadRecentIssues(data, consignmentIds);

        // Branch Capacities
        await LoadBranchCapacities(data, branchId);

        // Vehicle Status Summary
        await LoadVehicleStatusSummary(data, branchId);

        // Pending Pickups (for KPI card)
        data.PendingPickups = data.PickupPerformance.Pending;

        return data;
    }

    private async Task LoadPickupPerformance(DashboardData data, long? branchId,
        IQueryable<long> consignmentIds, DateTime todayStart, DateTime tomorrowStart)
    {
        var pickupQuery = _pickupTaskRepo.GetQueryable()
            .Where(p => consignmentIds.Contains(p.ConsignmentId));

        var statusGroups = await pickupQuery
            .GroupBy(p => p.TaskStatus)
            .Select(g => new { Status = g.Key, Count = g.LongCount() })
            .ToListAsync();

        data.PickupPerformance = new PickupPerformanceData
        {
            Pending = statusGroups
                .Where(x => x.Status == PickupTaskStatus.Pending || x.Status == PickupTaskStatus.Assigned)
                .Sum(x => x.Count),
            Completed = statusGroups
                .FirstOrDefault(x => x.Status == PickupTaskStatus.Completed)?.Count ?? 0,
            Failed = statusGroups
                .FirstOrDefault(x => x.Status == PickupTaskStatus.Failed)?.Count ?? 0,
            Cancelled = statusGroups
                .FirstOrDefault(x => x.Status == PickupTaskStatus.Cancelled)?.Count ?? 0,
            ScheduledToday = await pickupQuery
                .CountAsync(p => p.PickupDate >= todayStart && p.PickupDate < tomorrowStart),
            CompletedToday = await pickupQuery
                .CountAsync(p => p.TaskStatus == PickupTaskStatus.Completed
                                 && p.PickupTime >= todayStart && p.PickupTime < tomorrowStart)
        };
    }

    private async Task LoadTripStatusSummary(DashboardData data, long? branchId)
    {
        var tripQuery = _tripRepo.GetQueryable();
        if (branchId.HasValue)
            tripQuery = tripQuery.Where(t =>
                t.FromBranchId == branchId.Value || t.ToBranchId == branchId.Value);

        data.TripStatusSummary = await tripQuery
            .GroupBy(t => t.TripStatus)
            .Select(g => new { Status = g.Key, Count = g.LongCount() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);
    }

    private async Task LoadRecentConsignments(DashboardData data,
        IQueryable<Entities.Consignment.Consignment> consignmentQuery)
    {
        data.RecentConsignments = await consignmentQuery
            .OrderByDescending(c => c.RecDate)
            .Take(10)
            .Select(c => new RecentConsignmentItem
            {
                Id = c.Id,
                TrackingNumber = c.TrackingNumber,
                SenderName = c.Sender.Name,
                ReceiverName = c.Receiver.Name,
                ServiceType = c.ServiceType,
                LatestStatus = c.StatusLogs
                    .OrderByDescending(sl => sl.RecDate)
                    .Select(sl => (ConsignmentStatusType?)sl.StatusType)
                    .FirstOrDefault(),
                RecDate = c.RecDate
            })
            .ToListAsync();
    }

    private async Task LoadUpcomingPickups(DashboardData data, IQueryable<long> consignmentIds,
        DateTime todayStart)
    {
        data.UpcomingPickups = await _pickupTaskRepo.GetQueryable()
            .Where(p => consignmentIds.Contains(p.ConsignmentId))
            .Where(p => (p.TaskStatus == PickupTaskStatus.Pending || p.TaskStatus == PickupTaskStatus.Assigned)
                        && p.PickupDate >= todayStart)
            .OrderBy(p => p.PickupDate)
            .ThenBy(p => p.PickupSlot)
            .Take(10)
            .Select(p => new UpcomingPickupItem
            {
                Id = p.Id,
                TrackingNumber = p.Consignment.TrackingNumber,
                PickupDate = p.PickupDate,
                PickupSlot = p.PickupSlot,
                TaskStatus = p.TaskStatus,
                AssignedDriverName = p.AssignedDriver != null ? p.AssignedDriver.Name : null
            })
            .ToListAsync();
    }

    private async Task LoadActiveTrips(DashboardData data, long? branchId)
    {
        var tripQuery = _tripRepo.GetQueryable();
        if (branchId.HasValue)
            tripQuery = tripQuery.Where(t =>
                t.FromBranchId == branchId.Value || t.ToBranchId == branchId.Value);

        data.ActiveTrips = await tripQuery
            .Where(t => t.TripStatus == TripStatus.Scheduled || t.TripStatus == TripStatus.InTransit)
            .OrderByDescending(t => t.ScheduledDeparture)
            .Take(10)
            .Select(t => new ActiveTripItem
            {
                Id = t.Id,
                TripNumber = t.TripNumber,
                TripType = t.TripType,
                TripStatus = t.TripStatus,
                FromBranchName = t.FromBranch.Name,
                ToBranchName = t.ToBranch.Name,
                TotalConsignments = t.TotalConsignments,
                ScheduledDeparture = t.ScheduledDeparture,
                DriverName = t.Driver.Name
            })
            .ToListAsync();
    }

    private async Task LoadRecentIssues(DashboardData data, IQueryable<long> consignmentIds)
    {
        data.RecentIssues = await _statusLogRepo.GetQueryable()
            .Where(sl => consignmentIds.Contains(sl.ConsignmentId))
            .Where(sl => IssueStatuses.Contains(sl.StatusType))
            .OrderByDescending(sl => sl.RecDate)
            .Take(10)
            .Select(sl => new IssueItem
            {
                ConsignmentId = sl.ConsignmentId,
                TrackingNumber = sl.Consignment.TrackingNumber,
                StatusType = sl.StatusType,
                Remarks = sl.Remarks,
                OccurredAt = sl.RecDate
            })
            .ToListAsync();
    }

    private async Task LoadBranchCapacities(DashboardData data, long? branchId)
    {
        var branchQuery = _branchRepo.GetQueryable()
            .Where(b => b.Status == StatusEnum.Active);

        if (branchId.HasValue)
            branchQuery = branchQuery.Where(b => b.Id == branchId.Value);

        data.BranchCapacities = await branchQuery
            .Select(b => new BranchCapacityItem
            {
                Id = b.Id,
                Name = b.Name,
                Code = b.Code,
                BranchType = b.BranchType,
                StorageCapacity = b.StorageCapacity ?? 0,
                CurrentLoad = b.CurrentLoad,
                UtilizationPercent = b.StorageCapacity.HasValue && b.StorageCapacity.Value > 0
                    ? (int)((b.CurrentLoad / b.StorageCapacity.Value) * 100)
                    : 0
            })
            .OrderByDescending(b => b.UtilizationPercent)
            .ToListAsync();
    }

    private async Task LoadVehicleStatusSummary(DashboardData data, long? branchId)
    {
        var vehicleQuery = _vehicleRepo.GetQueryable();
        if (branchId.HasValue)
            vehicleQuery = vehicleQuery.Where(v => v.CurrentBranchId == branchId.Value);

        data.VehicleStatusSummary = await vehicleQuery
            .GroupBy(v => v.VehicleStatus)
            .Select(g => new { Status = g.Key, Count = g.LongCount() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);
    }
}
