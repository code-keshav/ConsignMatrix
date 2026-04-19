using Base.Dtos.Consignment;
using Base.Enum;
using Base.Enum.Consignment;
using Base.Providers.Interfaces;
using Base.Repo.Consignment.Interfaces;
using Base.Repo.Interfaces;
using Base.Services.Consignment.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Areas.Consignment.ViewModels.Pickup;
using Web.Helpers;

namespace Web.Areas.Consignment.Controllers;

[Area("Consignment")]
public class PickupTaskController : Controller
{
    private readonly INotificationHelper _notificationHelper;
    private readonly IPickupTaskService _pickupTaskService;
    private readonly IPickupTaskRepo _pickupTaskRepo;
    private readonly IConsignmentRepo _consignmentRepo;
    private readonly IConsignmentStatusLogRepo _statusLogRepo;
    private readonly IEmployeeRepo _employeeRepo;
    private readonly IVehicleRepo _vehicleRepo;
    private readonly ICurrentUserProvider _currentUserProvider;

    public PickupTaskController(
        INotificationHelper notificationHelper,
        IPickupTaskService pickupTaskService,
        IPickupTaskRepo pickupTaskRepo,
        IConsignmentRepo consignmentRepo,
        IConsignmentStatusLogRepo statusLogRepo,
        IEmployeeRepo employeeRepo,
        IVehicleRepo vehicleRepo,
        ICurrentUserProvider currentUserProvider)
    {
        _notificationHelper = notificationHelper;
        _pickupTaskService = pickupTaskService;
        _pickupTaskRepo = pickupTaskRepo;
        _consignmentRepo = consignmentRepo;
        _statusLogRepo = statusLogRepo;
        _employeeRepo = employeeRepo;
        _vehicleRepo = vehicleRepo;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var cu = await _currentUserProvider.GetCurrentUser();
        var vm = new PickupIndexVm
        {
            FilterDate = DateTime.Today,
            CurrentBranchId = cu.BranchId,
        };
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> GetPickupTasks(DateTime? date, int? status, int? slot, long? driverId)
    {
        try
        {
            var cu = await _currentUserProvider.GetCurrentUser();
            var queryable = _pickupTaskRepo.GetQueryable();

            if (date.HasValue)
                queryable = queryable.Where(p => p.PickupDate.Date == date.Value.Date);
            else
                queryable = queryable.Where(p => p.PickupDate.Date == DateTime.UtcNow.Date);

            if (status.HasValue)
                queryable = queryable.Where(p => (int)p.TaskStatus == status.Value);

            if (slot.HasValue)
                queryable = queryable.Where(p => (int)p.PickupSlot == slot.Value);

            if (driverId.HasValue)
                queryable = queryable.Where(p => p.AssignedDriverId == driverId.Value);

            // Filter by current user's branch (consignment origin)
            queryable = queryable.Where(p => p.Consignment.OriginBranchId == cu.BranchId);

            var tasks = await queryable.OrderBy(p => p.PickupSlot).ThenBy(p => p.RecDate).ToListAsync();

            var result = tasks.Select(t => new
            {
                t.Id,
                trackingNumber = t.Consignment?.TrackingNumber,
                senderName = t.Consignment?.Sender?.Name,
                senderPhone = t.Consignment?.Sender?.PhoneNo,
                t.PickupAddress,
                t.ContactPhone,
                t.ContactName,
                pickupDate = t.PickupDate.ToString("yyyy-MM-dd"),
                pickupSlot = t.PickupSlot.ToString(),
                pickupSlotValue = (int)t.PickupSlot,
                taskStatus = t.TaskStatus.ToString(),
                taskStatusValue = (int)t.TaskStatus,
                driverName = t.AssignedDriver?.Name,
                vehicleNumber = t.AssignedVehicle?.VehicleNumber,
                t.AttemptCount,
                consignmentWeight = t.Consignment?.ChargeableWeight ?? 0,
                packageCount = t.Consignment?.PackageCount ?? 0,
                consignmentId = t.ConsignmentId,
            }).ToList();

            // Summary counts
            var allForDate = await _pickupTaskRepo.GetQueryable()
                .Where(p => p.PickupDate.Date == (date ?? DateTime.UtcNow).Date
                            && p.Consignment.OriginBranchId == cu.BranchId)
                .ToListAsync();

            var summary = new
            {
                pending = allForDate.Count(t => t.TaskStatus == PickupTaskStatus.Pending),
                assigned = allForDate.Count(t => t.TaskStatus == PickupTaskStatus.Assigned),
                inProgress = allForDate.Count(t => t.TaskStatus == PickupTaskStatus.InProgress),
                completed = allForDate.Count(t => t.TaskStatus == PickupTaskStatus.Completed),
                failed = allForDate.Count(t => t.TaskStatus == PickupTaskStatus.Failed),
                total = allForDate.Count,
            };

            return Json(new { success = true, tasks = result, summary });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailableDrivers(long branchId)
    {
        try
        {
            var drivers = await _employeeRepo.FindByAsync(e =>
                e.EmployeeType == EmployeeType.Driver
                && e.EmployeeStatus == EmployeeStatus.Active
                && e.CurrentBranchId == branchId);
            return Json(drivers.Select(d => new
            {
                d.Id,
                d.Name,
                d.Phone,
                d.EmployeeCode,
            }));
        }
        catch
        {
            return Json(new List<object>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailableVehicles(long branchId)
    {
        try
        {
            var vehicles = await _vehicleRepo.FindByAsync(v =>
                v.VehicleStatus == VehicleStatus.Available
                && v.CurrentBranchId == branchId);
            return Json(vehicles.Select(v => new
            {
                v.Id,
                v.VehicleNumber,
                vehicleType = v.VehicleType.ToString(),
                v.MaxWeightCapacity,
                v.MaxVolumeCapacity,
            }));
        }
        catch
        {
            return Json(new List<object>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create(long? consignmentId)
    {
        try
        {
            var vm = new PickupTaskCreateVm
            {
                PickupDate = DateTime.Today.AddDays(1),
                PickupSlot = PickupSlot.Morning,
            };

            if (consignmentId.HasValue)
            {
                var consignment = await _consignmentRepo.FindOrThrowAsync(consignmentId.Value);
                var senderAddress = consignment.SenderAddress;
                vm.ConsignmentId = consignment.Id;
                vm.TrackingNumber = consignment.TrackingNumber;
                vm.SenderName = consignment.Sender?.Name;
                vm.SenderPhone = consignment.Sender?.PhoneNo;
                vm.TotalWeight = consignment.ChargeableWeight;
                vm.PackageCount = consignment.PackageCount;

                if (senderAddress != null)
                {
                    vm.PickupAddress = $"{senderAddress.AddressLine1}, {senderAddress.City}, {senderAddress.State} - {senderAddress.PinCode}";
                    vm.ContactPhone = senderAddress.ContactNo ?? consignment.Sender?.PhoneNo ?? "";
                    vm.ContactName = consignment.Sender?.Name;
                    vm.SenderAddress = vm.PickupAddress;
                }
            }

            return View(vm);
        }
        catch (Exception ex)
        {
            _notificationHelper.SetErrorMsg(ex.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PickupTaskCreateVm vm)
    {
        try
        {
            var dto = new PickupTaskCreateDto
            {
                ConsignmentId = vm.ConsignmentId,
                PickupDate = vm.PickupDate,
                PickupSlot = vm.PickupSlot,
                PickupAddress = vm.PickupAddress,
                ContactPhone = vm.ContactPhone,
                ContactName = vm.ContactName,
            };

            await _pickupTaskService.Create(dto);
            return Json(new { success = true, redirectUrl = Url.Action(nameof(Index)) });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Detail(long id)
    {
        try
        {
            var task = await _pickupTaskRepo.FindOrThrowAsync(id);
            var consignment = task.Consignment;
            var allTasks = await _pickupTaskRepo.GetByConsignmentId(task.ConsignmentId);

            var vm = new PickupDetailVm
            {
                Id = task.Id,
                ConsignmentId = task.ConsignmentId,
                TrackingNumber = consignment.TrackingNumber,
                SenderName = consignment.Sender?.Name ?? "",
                SenderPhone = consignment.Sender?.PhoneNo ?? "",
                PickupAddress = task.PickupAddress,
                ContactPhone = task.ContactPhone,
                ContactName = task.ContactName,
                PickupDate = task.PickupDate,
                PickupSlot = task.PickupSlot,
                TaskStatus = task.TaskStatus,
                AssignedDriverName = task.AssignedDriver?.Name,
                AssignedVehicleNumber = task.AssignedVehicle?.VehicleNumber,
                AttemptCount = task.AttemptCount,
                PickupTime = task.PickupTime,
                VerifiedWeight = task.VerifiedWeight,
                FailReason = task.FailReason,
                Remarks = task.Remarks,
                ConsignmentWeight = consignment.ChargeableWeight,
                PackageCount = consignment.PackageCount,
                ServiceType = consignment.ServiceType.ToString(),
                PaymentMode = consignment.PaymentMode.ToString(),
                RecDate = task.RecDate,
                HasActivePickup = allTasks.Any(t =>
                    t.TaskStatus != PickupTaskStatus.Completed
                    && t.TaskStatus != PickupTaskStatus.Failed
                    && t.TaskStatus != PickupTaskStatus.Cancelled),
                AttemptHistory = allTasks.Select(t => new PickupAttemptVm
                {
                    Id = t.Id,
                    PickupDate = t.PickupDate,
                    PickupSlot = t.PickupSlot,
                    TaskStatus = t.TaskStatus,
                    DriverName = t.AssignedDriver?.Name,
                    FailReason = t.FailReason,
                    Remarks = t.Remarks,
                    RecDate = t.RecDate,
                }).ToList(),
            };

            return View(vm);
        }
        catch (Exception ex)
        {
            _notificationHelper.SetErrorMsg(ex.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Assign([FromBody] PickupAssignVm vm)
    {
        try
        {
            var dto = new PickupAssignDto
            {
                PickupTaskId = vm.PickupTaskId,
                DriverId = vm.DriverId,
                VehicleId = vm.VehicleId,
            };
            await _pickupTaskService.Assign(dto);
            return Json(new { success = true, message = "Pickup task assigned successfully." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> BulkAssign([FromBody] PickupBulkAssignVm vm)
    {
        try
        {
            var dto = new PickupBulkAssignDto
            {
                PickupTaskIds = vm.PickupTaskIds,
                DriverId = vm.DriverId,
                VehicleId = vm.VehicleId,
            };
            await _pickupTaskService.BulkAssign(dto);
            return Json(new { success = true, message = $"{vm.PickupTaskIds.Count} pickup tasks assigned successfully." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> MarkInProgress(long id)
    {
        try
        {
            await _pickupTaskService.MarkInProgress(id);
            return Json(new { success = true, message = "Pickup started." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Complete([FromBody] PickupCompleteVm vm)
    {
        try
        {
            var dto = new PickupCompleteDto
            {
                PickupTaskId = vm.PickupTaskId,
                VerifiedWeight = vm.VerifiedWeight,
                Remarks = vm.Remarks,
            };
            await _pickupTaskService.Complete(dto);
            return Json(new { success = true, message = "Pickup completed successfully." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Fail([FromBody] PickupFailVm vm)
    {
        try
        {
            var dto = new PickupFailDto
            {
                PickupTaskId = vm.PickupTaskId,
                FailReason = vm.FailReason,
                Remarks = vm.Remarks,
                Reschedule = vm.Reschedule,
                RescheduleDate = vm.RescheduleDate,
                RescheduleSlot = vm.RescheduleSlot,
            };
            await _pickupTaskService.Fail(dto);
            return Json(new { success = true, message = "Pickup marked as failed." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Cancel(long id, string? reason)
    {
        try
        {
            await _pickupTaskService.Cancel(id, reason);
            return Json(new { success = true, message = "Pickup cancelled." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> SearchConsignment(string term)
    {
        try
        {
            var search = term.Trim().ToLower();
            var consignments = await _consignmentRepo.GetQueryable()
                .Where(c => c.TrackingNumber.ToLower().Contains(search))
                .Take(10)
                .ToListAsync();

            return Json(consignments.Select(c => new
            {
                c.Id,
                c.TrackingNumber,
                senderName = c.Sender?.Name,
                senderPhone = c.Sender?.PhoneNo,
                senderAddress = c.SenderAddress != null
                    ? $"{c.SenderAddress.AddressLine1}, {c.SenderAddress.City}, {c.SenderAddress.State} - {c.SenderAddress.PinCode}"
                    : "",
                contactPhone = c.SenderAddress?.ContactNo ?? c.Sender?.PhoneNo ?? "",
                chargeableWeight = c.ChargeableWeight,
                packageCount = c.PackageCount,
            }));
        }
        catch
        {
            return Json(new List<object>());
        }
    }
}
