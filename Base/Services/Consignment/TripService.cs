using System.Transactions;
using Base.Dtos.Consignment;
using Base.Entities.Consignment;
using Base.Enum;
using Base.Enum.Consignment;
using Base.Providers.Interfaces;
using Base.Repo.Consignment.Interfaces;
using Base.Repo.Interfaces;
using Base.Services.Consignment.Interfaces;

namespace Base.Services.Consignment;

public class TripService : ITripService
{
    private readonly IUow _uow;
    private readonly ITripRepo _tripRepo;
    private readonly ITripConsignmentRepo _tripConsignmentRepo;
    private readonly IConsignmentRepo _consignmentRepo;
    private readonly IConsignmentStatusLogRepo _statusLogRepo;
    private readonly IConsignmentStatusLogService _statusLogService;
    private readonly IEmployeeRepo _employeeRepo;
    private readonly IVehicleRepo _vehicleRepo;
    private readonly ICurrentUserProvider _currentUserProvider;

    public TripService(IUow uow, ITripRepo tripRepo, ITripConsignmentRepo tripConsignmentRepo,
        IConsignmentRepo consignmentRepo,
        IConsignmentStatusLogRepo statusLogRepo, IConsignmentStatusLogService statusLogService,
        IEmployeeRepo employeeRepo, IVehicleRepo vehicleRepo,
        ICurrentUserProvider currentUserProvider)
    {
        _uow = uow;
        _tripRepo = tripRepo;
        _tripConsignmentRepo = tripConsignmentRepo;
        _consignmentRepo = consignmentRepo;
        _statusLogRepo = statusLogRepo;
        _statusLogService = statusLogService;
        _employeeRepo = employeeRepo;
        _vehicleRepo = vehicleRepo;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<Trip> Create(TripCreateDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        if (dto.ConsignmentIds == null || dto.ConsignmentIds.Count == 0)
            throw new Exception("At least one consignment must be selected for dispatch.");

        if (dto.ToBranchId <= 0)
            throw new Exception("Destination branch is required.");

        var isDelivery = dto.TripType == TripType.Delivery;

        // Delivery trips must start and end at the same branch
        if (isDelivery && dto.FromBranchId != dto.ToBranchId)
            throw new Exception("Delivery trips must start and end at the same branch.");

        // Validate driver
        var driver = await _employeeRepo.FindOrThrowAsync(dto.DriverId);
        if (driver.EmployeeType != EmployeeType.Driver)
            throw new Exception("Selected employee is not a driver.");
        if (driver.EmployeeStatus != EmployeeStatus.Active)
            throw new Exception("Selected driver is not active.");
        if (driver.CurrentBranchId != dto.FromBranchId)
            throw new Exception("Driver must be assigned to the origin branch.");

        // Validate vehicle
        var vehicle = await _vehicleRepo.FindOrThrowAsync(dto.VehicleId);
        if (vehicle.VehicleStatus != VehicleStatus.Available)
            throw new Exception("Selected vehicle is not available.");
        if (vehicle.CurrentBranchId != dto.FromBranchId)
            throw new Exception("Vehicle must be at the origin branch.");

        // Validate consignment statuses based on trip type
        var requiredStatus = isDelivery
            ? ConsignmentStatusType.ReceivedAtDestination
            : ConsignmentStatusType.Bagged;

        decimal totalWeight = 0;
        var consignments = new List<Entities.Consignment.Consignment>();
        foreach (var id in dto.ConsignmentIds)
        {
            var consignment = await _consignmentRepo.FindOrThrowAsync(id);
            var latest = await _statusLogRepo.GetLatestStatus(id);

            // Check consignment is not already in another active trip
            if (await _tripConsignmentRepo.IsInActiveTripAsync(id))
                throw new Exception(
                    $"Consignment {consignment.TrackingNumber} is already assigned to an active trip.");

            // Delivery trips also accept DeliveryAttempted (re-dispatch after a failed attempt)
            var validForDelivery = isDelivery &&
                (latest?.StatusType == ConsignmentStatusType.ReceivedAtDestination ||
                 latest?.StatusType == ConsignmentStatusType.DeliveryAttempted);

            if (!isDelivery && latest?.StatusType != ConsignmentStatusType.Bagged)
                throw new Exception(
                    $"Consignment {consignment.TrackingNumber} must be in Bagged status to be dispatched.");

            if (isDelivery && !validForDelivery)
                throw new Exception(
                    $"Consignment {consignment.TrackingNumber} must be in ReceivedAtDestination or DeliveryAttempted status to be added to a delivery trip.");

            totalWeight += consignment.ChargeableWeight;
            consignments.Add(consignment);
        }

        var tripNumber = await _tripRepo.GenerateTripNumber();

        var trip = new Trip
        {
            TripNumber = tripNumber,
            TripType = dto.TripType,
            FromBranchId = dto.FromBranchId,
            ToBranchId = dto.ToBranchId,
            DriverId = dto.DriverId,
            VehicleId = dto.VehicleId,
            ScheduledDeparture = dto.ScheduledDeparture,
            TripStatus = TripStatus.Scheduled,
            TotalConsignments = dto.ConsignmentIds.Count,
            TotalWeight = totalWeight,
            Notes = dto.Notes,
        };

        await _uow.CreateAsync(trip);
        await _uow.CommitAsync();

        var loadedAt = DateTime.UtcNow;
        foreach (var consignment in consignments)
        {
            var tripConsignment = new TripConsignment
            {
                TripId = trip.Id,
                ConsignmentId = consignment.Id,
                LoadedAt = loadedAt,
            };
            await _uow.CreateAsync(tripConsignment);

            // Only non-Delivery trips transition consignments on Create.
            // Delivery trips keep consignments at ReceivedAtDestination until the trip is started.
            if (!isDelivery)
            {
                await _statusLogService.AddStatus(consignment.Id, ConsignmentStatusType.Dispatched,
                    $"Dispatched on trip {tripNumber}");
            }
        }

        await _uow.CommitAsync();
        tx.Complete();
        return trip;
    }

    public async Task StartTrip(long tripId)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var trip = await _tripRepo.FindOrThrowAsync(tripId);
        if (trip.TripStatus != TripStatus.Scheduled)
            throw new Exception("Only scheduled trips can be started.");

        // Only the origin branch can start a trip
        var currentBranchId = await _currentUserProvider.GetUserBranchId();
        if (trip.FromBranchId != currentBranchId)
            throw new Exception("Only the origin branch can start this trip.");

        trip.TripStatus = TripStatus.InTransit;
        trip.ActualDeparture = DateTime.UtcNow;

        _uow.Update(trip);
        await _uow.CommitAsync();

        // Update consignment statuses
        var isDelivery = trip.TripType == TripType.Delivery;
        var newStatus = isDelivery
            ? ConsignmentStatusType.OutForDelivery
            : ConsignmentStatusType.InTransit;

        var fromBranchName = trip.FromBranch?.Name ?? "origin branch";
        var remarks = isDelivery
            ? $"Out for delivery on trip {trip.TripNumber}"
            : $"Trip {trip.TripNumber} departed {fromBranchName}";

        foreach (var tc in trip.TripConsignments)
        {
            await _statusLogService.AddStatus(tc.ConsignmentId, newStatus, remarks);
        }

        tx.Complete();
    }

    public async Task CompleteTrip(long tripId)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var trip = await _tripRepo.FindOrThrowAsync(tripId);
        if (trip.TripStatus != TripStatus.InTransit)
            throw new Exception("Only in-transit trips can be completed.");

        var isDelivery = trip.TripType == TripType.Delivery;
        var currentBranchId = await _currentUserProvider.GetUserBranchId();

        // Delivery trips are completed by the origin branch (same as from/to).
        // Non-Delivery trips are completed by the destination branch (receiver).
        var expectedBranchId = isDelivery ? trip.FromBranchId : trip.ToBranchId;
        if (expectedBranchId != currentBranchId)
        {
            var role = isDelivery ? "origin" : "destination";
            throw new Exception($"Only the {role} branch can complete this trip.");
        }

        // Validate every consignment has been processed
        foreach (var tc in trip.TripConsignments)
        {
            var latest = await _statusLogRepo.GetLatestStatus(tc.ConsignmentId);
            var status = latest?.StatusType;

            if (isDelivery)
            {
                if (status != ConsignmentStatusType.Delivered &&
                    status != ConsignmentStatusType.DeliveryAttempted)
                {
                    throw new Exception(
                        $"Consignment {tc.Consignment?.TrackingNumber ?? tc.ConsignmentId.ToString()} must be marked delivered or failed before completing the delivery trip.");
                }
            }
            else
            {
                if (status != ConsignmentStatusType.ReceivedAtDestination &&
                    status != ConsignmentStatusType.Damaged &&
                    status != ConsignmentStatusType.Lost)
                {
                    throw new Exception(
                        $"Consignment {tc.Consignment?.TrackingNumber ?? tc.ConsignmentId.ToString()} must be scanned (received/damaged/missing) before completing the trip.");
                }
            }
        }

        trip.TripStatus = TripStatus.Completed;

        _uow.Update(trip);
        await _uow.CommitAsync();
        tx.Complete();
    }

    public async Task CancelTrip(long tripId)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var trip = await _tripRepo.FindOrThrowAsync(tripId);
        if (trip.TripStatus != TripStatus.Scheduled)
            throw new Exception("Only scheduled trips can be cancelled.");

        // Only the origin branch can cancel a trip
        var currentBranchId = await _currentUserProvider.GetUserBranchId();
        if (trip.FromBranchId != currentBranchId)
            throw new Exception("Only the origin branch can cancel this trip.");

        trip.TripStatus = TripStatus.Cancelled;
        _uow.Update(trip);
        await _uow.CommitAsync();

        var isDelivery = trip.TripType == TripType.Delivery;

        // Revert consignments — non-Delivery back to Bagged, Delivery keeps ReceivedAtDestination
        if (!isDelivery)
        {
            var tripConsignments = trip.TripConsignments;
            foreach (var tc in tripConsignments)
            {
                await _statusLogService.AddStatus(tc.ConsignmentId, ConsignmentStatusType.Bagged,
                    $"Trip {trip.TripNumber} cancelled — consignment returned to bagged status");
            }
        }

        tx.Complete();
    }

    public async Task ReceiveTripItem(TripReceiveItemDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var trip = await _tripRepo.FindOrThrowAsync(dto.TripId);
        if (trip.TripStatus != TripStatus.InTransit)
            throw new Exception("Trip must be in transit to receive items.");

        if (trip.TripType == TripType.Delivery)
            throw new Exception("Delivery trips cannot be received — use Mark Delivered / Mark Failed.");

        var currentBranchId = await _currentUserProvider.GetUserBranchId();
        if (trip.ToBranchId != currentBranchId)
            throw new Exception("Only the destination branch can receive trip items.");

        var tripConsignment = trip.TripConsignments.FirstOrDefault(tc => tc.ConsignmentId == dto.ConsignmentId);
        if (tripConsignment == null)
            throw new Exception("Consignment is not part of this trip's manifest.");

        var latest = await _statusLogRepo.GetLatestStatus(dto.ConsignmentId);
        if (latest?.StatusType != ConsignmentStatusType.InTransit)
            throw new Exception("Consignment has already been processed or is not in transit.");

        ConsignmentStatusType newStatus;
        string remarks;
        switch (dto.Action)
        {
            case TripReceiveAction.Received:
                newStatus = ConsignmentStatusType.ReceivedAtDestination;
                remarks = $"Received at destination via trip {trip.TripNumber}.";
                break;
            case TripReceiveAction.Damaged:
                newStatus = ConsignmentStatusType.Damaged;
                remarks = $"Damaged on arrival via trip {trip.TripNumber}.";
                break;
            case TripReceiveAction.Missing:
                newStatus = ConsignmentStatusType.Lost;
                remarks = $"Missing from trip {trip.TripNumber} manifest.";
                break;
            default:
                throw new Exception("Invalid receive action.");
        }

        if (!string.IsNullOrWhiteSpace(dto.Remarks))
            remarks += $" {dto.Remarks.Trim()}";

        await _statusLogService.AddStatus(dto.ConsignmentId, newStatus, remarks);

        tx.Complete();
    }

    public async Task MarkDelivered(MarkDeliveredDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        if (string.IsNullOrWhiteSpace(dto.ReceiverName))
            throw new Exception("Receiver name is required.");

        var trip = await _tripRepo.FindOrThrowAsync(dto.TripId);
        if (trip.TripType != TripType.Delivery)
            throw new Exception("This action is only valid on delivery trips.");
        if (trip.TripStatus != TripStatus.InTransit)
            throw new Exception("Delivery trip must be in transit.");

        var currentBranchId = await _currentUserProvider.GetUserBranchId();
        if (trip.FromBranchId != currentBranchId)
            throw new Exception("Only the branch running the delivery can update delivery status.");

        var tripConsignment = trip.TripConsignments.FirstOrDefault(tc => tc.ConsignmentId == dto.ConsignmentId);
        if (tripConsignment == null)
            throw new Exception("Consignment is not part of this delivery trip.");

        var latest = await _statusLogRepo.GetLatestStatus(dto.ConsignmentId);
        if (latest?.StatusType != ConsignmentStatusType.OutForDelivery)
            throw new Exception("Consignment is not out for delivery.");

        var remarks = $"Delivered to {dto.ReceiverName.Trim()}.";
        if (!string.IsNullOrWhiteSpace(dto.Remarks))
            remarks += $" {dto.Remarks.Trim()}";

        await _statusLogService.AddStatus(dto.ConsignmentId, ConsignmentStatusType.Delivered, remarks);

        tx.Complete();
    }

    public async Task MarkDeliveryFailed(MarkDeliveryFailedDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var trip = await _tripRepo.FindOrThrowAsync(dto.TripId);
        if (trip.TripType != TripType.Delivery)
            throw new Exception("This action is only valid on delivery trips.");
        if (trip.TripStatus != TripStatus.InTransit)
            throw new Exception("Delivery trip must be in transit.");

        var currentBranchId = await _currentUserProvider.GetUserBranchId();
        if (trip.FromBranchId != currentBranchId)
            throw new Exception("Only the branch running the delivery can update delivery status.");

        var tripConsignment = trip.TripConsignments.FirstOrDefault(tc => tc.ConsignmentId == dto.ConsignmentId);
        if (tripConsignment == null)
            throw new Exception("Consignment is not part of this delivery trip.");

        var latest = await _statusLogRepo.GetLatestStatus(dto.ConsignmentId);
        if (latest?.StatusType != ConsignmentStatusType.OutForDelivery)
            throw new Exception("Consignment is not out for delivery.");

        var remarks = $"Failed: {dto.FailReason}.";
        if (!string.IsNullOrWhiteSpace(dto.Remarks))
            remarks += $" {dto.Remarks.Trim()}";

        await _statusLogService.AddStatus(dto.ConsignmentId, ConsignmentStatusType.DeliveryAttempted, remarks);

        tx.Complete();
    }
}
