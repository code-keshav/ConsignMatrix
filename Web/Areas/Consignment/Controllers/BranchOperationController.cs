using Base.Dtos.Consignment;
using Base.Entities;
using Base.Enum;
using Base.Enum.Consignment;
using Base.Providers.Interfaces;
using Base.Repo.Consignment.Interfaces;
using Base.Repo.Interfaces;
using Base.Services.Consignment.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Areas.Consignment.ViewModels.BranchOperation;
using Web.Helpers;

namespace Web.Areas.Consignment.Controllers;

[Area("Consignment")]
public class BranchOperationController : Controller
{
    private readonly INotificationHelper _notificationHelper;
    private readonly IBranchOperationService _branchOperationService;
    private readonly IConsignmentRepo _consignmentRepo;
    private readonly IConsignmentStatusLogRepo _statusLogRepo;
    private readonly IEmployeeRepo _employeeRepo;
    private readonly IVehicleRepo _vehicleRepo;
    private readonly IBranchRepo _branchRepo;
    private readonly ICurrentUserProvider _currentUserProvider;

    public BranchOperationController(
        INotificationHelper notificationHelper,
        IBranchOperationService branchOperationService,
        IConsignmentRepo consignmentRepo,
        IConsignmentStatusLogRepo statusLogRepo,
        IEmployeeRepo employeeRepo,
        IVehicleRepo vehicleRepo,
        IBranchRepo branchRepo,
        ICurrentUserProvider currentUserProvider)
    {
        _notificationHelper = notificationHelper;
        _branchOperationService = branchOperationService;
        _consignmentRepo = consignmentRepo;
        _statusLogRepo = statusLogRepo;
        _employeeRepo = employeeRepo;
        _vehicleRepo = vehicleRepo;
        _branchRepo = branchRepo;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var branches = await _branchRepo.FindByAsync(b => b.Status == StatusEnum.Active);
        var vm = new BranchOperationIndexVm
        {
            Branches = branches.Select(b => new BranchSelectItem
            {
                Id = b.Id,
                Name = b.Name,
                Code = b.Code,
            }).ToList()
        };
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> GetInwardConsignments(long? branchId, string? trackingNum)
    {
        try
        {
            var cu = await _currentUserProvider.GetCurrentUser();
            var effectiveBranchId = branchId ?? cu.BranchId;

            var consignments = await _consignmentRepo.GetQueryable()
                .Where(c => c.OriginBranchId == effectiveBranchId)
                .OrderByDescending(c => c.RecDate)
                .ToListAsync();

            var result = new List<object>();
            foreach (var c in consignments)
            {
                var latest = await _statusLogRepo.GetLatestStatus(c.Id);
                if (latest?.StatusType != ConsignmentStatusType.Booked &&
                    latest?.StatusType != ConsignmentStatusType.PickedUp)
                    continue;

                if (!string.IsNullOrWhiteSpace(trackingNum) &&
                    !c.TrackingNumber.Contains(trackingNum.Trim(), StringComparison.OrdinalIgnoreCase))
                    continue;

                result.Add(new
                {
                    c.Id,
                    c.TrackingNumber,
                    senderName = c.Sender?.Name,
                    receiverName = c.Receiver?.Name,
                    destinationBranch = c.DestinationBranch?.Name,
                    destinationBranchId = c.DestinationBranchId,
                    c.ChargeableWeight,
                    c.PackageCount,
                    currentStatus = latest?.StatusType.ToString(),
                    currentStatusValue = (int)(latest?.StatusType ?? ConsignmentStatusType.Booked),
                    recDate = c.RecDate.ToString("yyyy-MM-dd HH:mm"),
                });
            }

            return Json(new { success = true, consignments = result });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> ReceiveConsignment([FromBody] ReceiveVm vm)
    {
        try
        {
            await _branchOperationService.ReceiveConsignment(new ReceiveConsignmentDto
            {
                ConsignmentId = vm.ConsignmentId,
                Remarks = vm.Remarks,
            });
            return Json(new { success = true, message = "Consignment received at origin branch." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> BulkReceive([FromBody] BulkReceiveVm vm)
    {
        try
        {
            await _branchOperationService.BulkReceive(new BulkReceiveDto
            {
                ConsignmentIds = vm.ConsignmentIds,
                Remarks = vm.Remarks,
            });
            return Json(new
            {
                success = true,
                message = $"{vm.ConsignmentIds.Count} consignment(s) received at origin branch."
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetSortingConsignments(long? branchId, string? trackingNum)
    {
        try
        {
            var cu = await _currentUserProvider.GetCurrentUser();
            var effectiveBranchId = branchId ?? cu.BranchId;

            var consignments = await _consignmentRepo.GetQueryable()
                .Where(c => c.OriginBranchId == effectiveBranchId)
                .OrderByDescending(c => c.RecDate)
                .ToListAsync();

            var result = new List<object>();
            foreach (var c in consignments)
            {
                var latest = await _statusLogRepo.GetLatestStatus(c.Id);
                if (latest?.StatusType != ConsignmentStatusType.ReceivedAtOrigin)
                    continue;

                if (!string.IsNullOrWhiteSpace(trackingNum) &&
                    !c.TrackingNumber.Contains(trackingNum.Trim(), StringComparison.OrdinalIgnoreCase))
                    continue;

                result.Add(new
                {
                    c.Id,
                    c.TrackingNumber,
                    senderName = c.Sender?.Name,
                    receiverName = c.Receiver?.Name,
                    destinationBranch = c.DestinationBranch?.Name,
                    destinationBranchId = c.DestinationBranchId,
                    c.ChargeableWeight,
                    c.PackageCount,
                    recDate = c.RecDate.ToString("yyyy-MM-dd HH:mm"),
                });
            }

            return Json(new { success = true, consignments = result });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SortConsignment([FromBody] SortVm vm)
    {
        try
        {
            await _branchOperationService.SortConsignment(new SortConsignmentDto
            {
                ConsignmentId = vm.ConsignmentId,
                DestinationBranchId = vm.DestinationBranchId,
                Remarks = vm.Remarks,
            });
            return Json(new { success = true, message = "Consignment sorted." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> BulkSort([FromBody] BulkSortVm vm)
    {
        try
        {
            await _branchOperationService.BulkSort(new BulkSortDto
            {
                Items = vm.Items.Select(i => new BulkSortItem
                {
                    ConsignmentId = i.ConsignmentId,
                    DestinationBranchId = i.DestinationBranchId,
                }).ToList(),
                Remarks = vm.Remarks,
            });
            return Json(new
            {
                success = true,
                message = $"{vm.Items.Count} consignment(s) sorted."
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetBaggingConsignments(long? branchId, long? destinationBranchId, string? trackingNum)
    {
        try
        {
            var cu = await _currentUserProvider.GetCurrentUser();
            var effectiveBranchId = branchId ?? cu.BranchId;

            var consignments = await _consignmentRepo.GetQueryable()
                .Where(c => c.OriginBranchId == effectiveBranchId)
                .OrderByDescending(c => c.RecDate)
                .ToListAsync();

            var result = new List<object>();
            foreach (var c in consignments)
            {
                var latest = await _statusLogRepo.GetLatestStatus(c.Id);
                if (latest?.StatusType != ConsignmentStatusType.Sorted)
                    continue;

                if (destinationBranchId.HasValue && c.DestinationBranchId != destinationBranchId.Value)
                    continue;

                if (!string.IsNullOrWhiteSpace(trackingNum) &&
                    !c.TrackingNumber.Contains(trackingNum.Trim(), StringComparison.OrdinalIgnoreCase))
                    continue;

                result.Add(new
                {
                    c.Id,
                    c.TrackingNumber,
                    senderName = c.Sender?.Name,
                    receiverName = c.Receiver?.Name,
                    destinationBranch = c.DestinationBranch?.Name,
                    destinationBranchId = c.DestinationBranchId,
                    c.ChargeableWeight,
                    c.PackageCount,
                    recDate = c.RecDate.ToString("yyyy-MM-dd HH:mm"),
                });
            }

            return Json(new { success = true, consignments = result });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> BagConsignment([FromBody] BagVm vm)
    {
        try
        {
            await _branchOperationService.BagConsignment(new BagConsignmentDto
            {
                ConsignmentId = vm.ConsignmentId,
                Remarks = vm.Remarks,
            });
            return Json(new { success = true, message = "Consignment bagged." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> BulkBag([FromBody] BulkBagVm vm)
    {
        try
        {
            await _branchOperationService.BulkBag(new BulkBagDto
            {
                ConsignmentIds = vm.ConsignmentIds,
                Remarks = vm.Remarks,
            });
            return Json(new
            {
                success = true,
                message = $"{vm.ConsignmentIds.Count} consignment(s) bagged."
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetDispatchConsignments(long? branchId, long? destinationBranchId)
    {
        try
        {
            var cu = await _currentUserProvider.GetCurrentUser();
            var effectiveBranchId = branchId ?? cu.BranchId;

            var consignments = await _consignmentRepo.GetQueryable()
                .Where(c => c.OriginBranchId == effectiveBranchId)
                .OrderByDescending(c => c.RecDate)
                .ToListAsync();

            var result = new List<object>();
            foreach (var c in consignments)
            {
                var latest = await _statusLogRepo.GetLatestStatus(c.Id);
                if (latest?.StatusType != ConsignmentStatusType.Bagged)
                    continue;

                if (destinationBranchId.HasValue && c.DestinationBranchId != destinationBranchId.Value)
                    continue;

                result.Add(new
                {
                    c.Id,
                    c.TrackingNumber,
                    senderName = c.Sender?.Name,
                    receiverName = c.Receiver?.Name,
                    destinationBranch = c.DestinationBranch?.Name,
                    destinationBranchId = c.DestinationBranchId,
                    c.ChargeableWeight,
                    c.PackageCount,
                    recDate = c.RecDate.ToString("yyyy-MM-dd HH:mm"),
                });
            }

            return Json(new { success = true, consignments = result });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailableDrivers(long? branchId)
    {
        try
        {
            var cu = await _currentUserProvider.GetCurrentUser();
            var effectiveBranchId = branchId ?? cu.BranchId;

            var drivers = await _employeeRepo.FindByAsync(e =>
                e.EmployeeType == EmployeeType.Driver
                && e.EmployeeStatus == EmployeeStatus.Active
                && e.CurrentBranchId == effectiveBranchId);

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
    public async Task<IActionResult> GetAvailableVehicles(long? branchId)
    {
        try
        {
            var cu = await _currentUserProvider.GetCurrentUser();
            var effectiveBranchId = branchId ?? cu.BranchId;

            var vehicles = await _vehicleRepo.FindByAsync(v =>
                v.VehicleStatus == VehicleStatus.Available
                && v.CurrentBranchId == effectiveBranchId);

            return Json(vehicles.Select(v => new
            {
                v.Id,
                v.VehicleNumber,
                vehicleType = v.VehicleType.ToString(),
                v.MaxWeightCapacity,
            }));
        }
        catch
        {
            return Json(new List<object>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllBranches()
    {
        try
        {
            var branches = await _branchRepo.FindByAsync(b => b.Status == StatusEnum.Active);
            return Json(branches.Select(b => new
            {
                b.Id,
                b.Name,
                b.Code,
            }));
        }
        catch
        {
            return Json(new List<object>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetSummary(long? branchId)
    {
        try
        {
            var cu = await _currentUserProvider.GetCurrentUser();
            var effectiveBranchId = branchId ?? cu.BranchId;

            var originConsignments = await _consignmentRepo.GetQueryable()
                .Where(c => c.OriginBranchId == effectiveBranchId)
                .ToListAsync();

            int toReceive = 0, sorting = 0, bagging = 0, readyToDispatch = 0;

            foreach (var c in originConsignments)
            {
                var latest = await _statusLogRepo.GetLatestStatus(c.Id);
                switch (latest?.StatusType)
                {
                    case ConsignmentStatusType.Booked:
                    case ConsignmentStatusType.PickedUp:
                        toReceive++; break;
                    case ConsignmentStatusType.ReceivedAtOrigin:
                        sorting++; break;
                    case ConsignmentStatusType.Sorted:
                        bagging++; break;
                    case ConsignmentStatusType.Bagged:
                        readyToDispatch++; break;
                }
            }

            // Ready-for-delivery consignments are those DESTINED to this branch
            var destinationConsignments = await _consignmentRepo.GetQueryable()
                .Where(c => c.DestinationBranchId == effectiveBranchId)
                .ToListAsync();

            int readyForDelivery = 0;
            foreach (var c in destinationConsignments)
            {
                var latest = await _statusLogRepo.GetLatestStatus(c.Id);
                if (latest?.StatusType == ConsignmentStatusType.ReceivedAtDestination ||
                    latest?.StatusType == ConsignmentStatusType.DeliveryAttempted)
                {
                    readyForDelivery++;
                }
            }

            return Json(new
            {
                success = true,
                toReceive,
                sorting,
                bagging,
                readyToDispatch,
                readyForDelivery,
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetReadyForDeliveryConsignments(long? branchId, string? trackingNum)
    {
        try
        {
            var cu = await _currentUserProvider.GetCurrentUser();
            var effectiveBranchId = branchId ?? cu.BranchId;

            var consignments = await _consignmentRepo.GetQueryable()
                .Where(c => c.DestinationBranchId == effectiveBranchId)
                .OrderByDescending(c => c.RecDate)
                .ToListAsync();

            var result = new List<object>();
            foreach (var c in consignments)
            {
                var latest = await _statusLogRepo.GetLatestStatus(c.Id);
                if (latest?.StatusType != ConsignmentStatusType.ReceivedAtDestination &&
                    latest?.StatusType != ConsignmentStatusType.DeliveryAttempted)
                    continue;

                if (!string.IsNullOrWhiteSpace(trackingNum) &&
                    !c.TrackingNumber.Contains(trackingNum.Trim(), StringComparison.OrdinalIgnoreCase))
                    continue;

                // Count prior delivery attempts
                var allStatuses = await _statusLogRepo.GetByConsignmentId(c.Id);
                var attempts = allStatuses.Count(s => s.StatusType == ConsignmentStatusType.DeliveryAttempted);

                result.Add(new
                {
                    c.Id,
                    c.TrackingNumber,
                    senderName = c.Sender?.Name,
                    receiverName = c.Receiver?.Name,
                    destinationBranch = c.DestinationBranch?.Name,
                    destinationBranchId = c.DestinationBranchId,
                    c.ChargeableWeight,
                    c.PackageCount,
                    currentStatus = latest?.StatusType.ToString(),
                    attempts,
                    recDate = c.RecDate.ToString("yyyy-MM-dd HH:mm"),
                });
            }

            return Json(new { success = true, consignments = result });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}
