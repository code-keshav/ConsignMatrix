using System.Transactions;
using Base.Dtos.Consignment;
using Base.Entities.Consignment;
using Base.Enum;
using Base.Enum.Consignment;
using Base.Repo.Consignment.Interfaces;
using Base.Repo.Interfaces;
using Base.Services.Consignment.Interfaces;

namespace Base.Services.Consignment;

public class PickupTaskService : IPickupTaskService
{
    private readonly IUow _uow;
    private readonly IPickupTaskRepo _pickupTaskRepo;
    private readonly IConsignmentRepo _consignmentRepo;
    private readonly IConsignmentStatusLogService _statusLogService;
    private readonly IConsignmentStatusLogRepo _statusLogRepo;
    private readonly IEmployeeRepo _employeeRepo;
    private readonly IVehicleRepo _vehicleRepo;

    public PickupTaskService(IUow uow, IPickupTaskRepo pickupTaskRepo, IConsignmentRepo consignmentRepo,
        IConsignmentStatusLogService statusLogService, IConsignmentStatusLogRepo statusLogRepo,
        IEmployeeRepo employeeRepo, IVehicleRepo vehicleRepo)
    {
        _uow = uow;
        _pickupTaskRepo = pickupTaskRepo;
        _consignmentRepo = consignmentRepo;
        _statusLogService = statusLogService;
        _statusLogRepo = statusLogRepo;
        _employeeRepo = employeeRepo;
        _vehicleRepo = vehicleRepo;
    }

    public async Task<PickupTask> Create(PickupTaskCreateDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var consignment = await _consignmentRepo.FindOrThrowAsync(dto.ConsignmentId);

        // Validate consignment status — only Booked or PickupAttempted (manual reschedule)
        var latest = await _statusLogRepo.GetLatestStatus(dto.ConsignmentId);
        if (latest?.StatusType != ConsignmentStatusType.Booked &&
            latest?.StatusType != ConsignmentStatusType.PickupAttempted)
            throw new Exception("Pickup can only be scheduled for consignments in Booked or PickupAttempted status.");

        // Check no active pickup task exists
        var existing = await _pickupTaskRepo.GetActiveByConsignmentId(dto.ConsignmentId);
        if (existing != null)
            throw new Exception("An active pickup task already exists for this consignment.");

        if (dto.PickupDate.Date < DateTime.UtcNow.Date)
            throw new Exception("Pickup date cannot be in the past.");

        if (string.IsNullOrWhiteSpace(dto.PickupAddress))
            throw new Exception("Pickup address is required.");

        if (string.IsNullOrWhiteSpace(dto.ContactPhone))
            throw new Exception("Contact phone is required.");

        var pickupTask = new PickupTask
        {
            ConsignmentId = dto.ConsignmentId,
            PickupDate = dto.PickupDate,
            PickupSlot = dto.PickupSlot,
            PickupAddress = dto.PickupAddress,
            ContactPhone = dto.ContactPhone,
            ContactName = dto.ContactName,
            TaskStatus = PickupTaskStatus.Pending,
            AttemptCount = 0,
        };

        await _uow.CreateAsync(pickupTask);
        await _uow.CommitAsync();

        await _statusLogService.AddStatus(dto.ConsignmentId, ConsignmentStatusType.PickupScheduled,
            $"Pickup scheduled for {dto.PickupDate:yyyy-MM-dd} ({dto.PickupSlot})");

        tx.Complete();
        return pickupTask;
    }

    public async Task Assign(PickupAssignDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var task = await _pickupTaskRepo.FindOrThrowAsync(dto.PickupTaskId);
        if (task.TaskStatus != PickupTaskStatus.Pending)
            throw new Exception("Only pending pickup tasks can be assigned.");

        await ValidateDriverAndVehicle(dto.DriverId, dto.VehicleId, task.Consignment.OriginBranchId);

        task.AssignedDriverId = dto.DriverId;
        task.AssignedVehicleId = dto.VehicleId;
        task.TaskStatus = PickupTaskStatus.Assigned;

        _uow.Update(task);
        await _uow.CommitAsync();
        tx.Complete();
    }

    public async Task BulkAssign(PickupBulkAssignDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        if (dto.PickupTaskIds == null || dto.PickupTaskIds.Count == 0)
            throw new Exception("At least one pickup task must be selected.");

        // Validate driver and vehicle once (use first task's branch for validation)
        var firstTask = await _pickupTaskRepo.FindOrThrowAsync(dto.PickupTaskIds.First());
        await ValidateDriverAndVehicle(dto.DriverId, dto.VehicleId, firstTask.Consignment.OriginBranchId);

        foreach (var taskId in dto.PickupTaskIds)
        {
            var task = await _pickupTaskRepo.FindOrThrowAsync(taskId);
            if (task.TaskStatus != PickupTaskStatus.Pending)
                throw new Exception($"Pickup task #{task.Id} is not in Pending status.");

            task.AssignedDriverId = dto.DriverId;
            task.AssignedVehicleId = dto.VehicleId;
            task.TaskStatus = PickupTaskStatus.Assigned;
            _uow.Update(task);
        }

        await _uow.CommitAsync();
        tx.Complete();
    }

    public async Task MarkInProgress(long pickupTaskId)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var task = await _pickupTaskRepo.FindOrThrowAsync(pickupTaskId);
        if (task.TaskStatus != PickupTaskStatus.Assigned)
            throw new Exception("Only assigned pickup tasks can be started.");

        task.TaskStatus = PickupTaskStatus.InProgress;
        _uow.Update(task);
        await _uow.CommitAsync();
        tx.Complete();
    }

    public async Task Complete(PickupCompleteDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var task = await _pickupTaskRepo.FindOrThrowAsync(dto.PickupTaskId);
        if (task.TaskStatus != PickupTaskStatus.InProgress && task.TaskStatus != PickupTaskStatus.Assigned)
            throw new Exception("Only assigned or in-progress pickup tasks can be completed.");

        // Validate consignment is still in PickupScheduled status
        var latest = await _statusLogRepo.GetLatestStatus(task.ConsignmentId);
        if (latest?.StatusType != ConsignmentStatusType.PickupScheduled)
            throw new Exception("Consignment must be in PickupScheduled status to complete pickup.");

        task.TaskStatus = PickupTaskStatus.Completed;
        task.PickupTime = DateTime.UtcNow;
        task.VerifiedWeight = dto.VerifiedWeight;
        task.Remarks = dto.Remarks;
        task.AttemptCount++;

        _uow.Update(task);
        await _uow.CommitAsync();

        await _statusLogService.AddStatus(task.ConsignmentId, ConsignmentStatusType.PickedUp,
            $"Pickup completed. Verified weight: {dto.VerifiedWeight}kg");

        tx.Complete();
    }

    public async Task Fail(PickupFailDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var task = await _pickupTaskRepo.FindOrThrowAsync(dto.PickupTaskId);
        if (task.TaskStatus != PickupTaskStatus.InProgress && task.TaskStatus != PickupTaskStatus.Assigned)
            throw new Exception("Only assigned or in-progress pickup tasks can be marked as failed.");

        // Validate consignment is still in PickupScheduled status
        var latest = await _statusLogRepo.GetLatestStatus(task.ConsignmentId);
        if (latest?.StatusType != ConsignmentStatusType.PickupScheduled)
            throw new Exception("Consignment must be in PickupScheduled status to mark pickup as failed.");

        task.TaskStatus = PickupTaskStatus.Failed;
        task.FailReason = dto.FailReason;
        task.Remarks = dto.Remarks;
        task.AttemptCount++;

        _uow.Update(task);
        await _uow.CommitAsync();

        var remarks = $"Pickup failed: {dto.FailReason}";
        if (task.AttemptCount >= 3)
            remarks += " - Max attempts reached, escalation required";

        await _statusLogService.AddStatus(task.ConsignmentId, ConsignmentStatusType.PickupAttempted, remarks);

        // Auto-reschedule if requested and under max attempts
        if (dto.Reschedule && task.AttemptCount < 3)
        {
            var rescheduleDate = dto.RescheduleDate ?? task.PickupDate.AddDays(1);
            var rescheduleSlot = dto.RescheduleSlot ?? task.PickupSlot;

            var newTask = new PickupTask
            {
                ConsignmentId = task.ConsignmentId,
                PickupDate = rescheduleDate,
                PickupSlot = rescheduleSlot,
                PickupAddress = task.PickupAddress,
                ContactPhone = task.ContactPhone,
                ContactName = task.ContactName,
                TaskStatus = PickupTaskStatus.Pending,
                AttemptCount = 0,
            };

            await _uow.CreateAsync(newTask);
            await _uow.CommitAsync();

            await _statusLogService.AddStatus(task.ConsignmentId, ConsignmentStatusType.PickupScheduled,
                $"Pickup rescheduled for {rescheduleDate:yyyy-MM-dd} ({rescheduleSlot})");
        }

        tx.Complete();
    }

    public async Task Cancel(long pickupTaskId, string? reason = null)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var task = await _pickupTaskRepo.FindOrThrowAsync(pickupTaskId);
        if (task.TaskStatus == PickupTaskStatus.Completed || task.TaskStatus == PickupTaskStatus.Cancelled)
            throw new Exception("Completed or already cancelled pickup tasks cannot be cancelled.");

        task.TaskStatus = PickupTaskStatus.Cancelled;
        task.Remarks = reason;

        _uow.Update(task);
        await _uow.CommitAsync();

        // Revert consignment status to Booked if it's still at PickupScheduled
        var latest = await _statusLogRepo.GetLatestStatus(task.ConsignmentId);
        if (latest?.StatusType == ConsignmentStatusType.PickupScheduled)
        {
            await _statusLogService.AddStatus(task.ConsignmentId, ConsignmentStatusType.Booked,
                "Pickup task cancelled — consignment reverted to Booked");
        }

        tx.Complete();
    }

    private async Task ValidateDriverAndVehicle(long driverId, long vehicleId, long originBranchId)
    {
        var driver = await _employeeRepo.FindOrThrowAsync(driverId);
        if (driver.EmployeeType != EmployeeType.Driver)
            throw new Exception("Selected employee is not a driver.");
        if (driver.EmployeeStatus != EmployeeStatus.Active)
            throw new Exception("Selected driver is not active.");
        if (driver.CurrentBranchId != originBranchId)
            throw new Exception("Driver must be assigned to the consignment's origin branch.");

        var vehicle = await _vehicleRepo.FindOrThrowAsync(vehicleId);
        if (vehicle.VehicleStatus != VehicleStatus.Available)
            throw new Exception("Selected vehicle is not available.");
        if (vehicle.CurrentBranchId != originBranchId)
            throw new Exception("Vehicle must be at the consignment's origin branch.");
    }
}
