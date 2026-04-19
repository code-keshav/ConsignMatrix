using Base.Dtos.Consignment;
using Base.Enum.Consignment;
using Base.Providers.Interfaces;
using Base.Repo.Consignment.Interfaces;
using Base.Repo.Interfaces;
using Base.Services.Consignment.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Areas.Consignment.ViewModels.Trip;
using Web.Helpers;

namespace Web.Areas.Consignment.Controllers;

[Area("Consignment")]
public class TripController : Controller
{
    private readonly INotificationHelper _notificationHelper;
    private readonly ITripService _tripService;
    private readonly ITripRepo _tripRepo;
    private readonly IConsignmentStatusLogRepo _statusLogRepo;
    private readonly IBranchRepo _branchRepo;
    private readonly ICurrentUserProvider _currentUserProvider;

    public TripController(
        INotificationHelper notificationHelper,
        ITripService tripService,
        ITripRepo tripRepo,
        IConsignmentStatusLogRepo statusLogRepo,
        IBranchRepo branchRepo,
        ICurrentUserProvider currentUserProvider)
    {
        _notificationHelper = notificationHelper;
        _tripService = tripService;
        _tripRepo = tripRepo;
        _statusLogRepo = statusLogRepo;
        _branchRepo = branchRepo;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet]
    public async Task<IActionResult> Index(TripStatus? status, TripType? tripType,
        DateTime? dateFrom, DateTime? dateTo, string? tripNumber,
        TripDirection direction = TripDirection.Outgoing)
    {
        try
        {
            var cu = await _currentUserProvider.GetCurrentUser();
            var queryable = _tripRepo.GetQueryable();

            switch (direction)
            {
                case TripDirection.Incoming:
                    // Exclude delivery runs (which are same-branch) from Incoming
                    queryable = queryable.Where(t =>
                        t.ToBranchId == cu.BranchId && t.FromBranchId != cu.BranchId);
                    break;
                case TripDirection.All:
                    queryable = queryable.Where(t =>
                        t.FromBranchId == cu.BranchId || t.ToBranchId == cu.BranchId);
                    break;
                case TripDirection.Outgoing:
                default:
                    queryable = queryable.Where(t => t.FromBranchId == cu.BranchId);
                    break;
            }

            if (status.HasValue)
                queryable = queryable.Where(t => t.TripStatus == status.Value);
            if (tripType.HasValue)
                queryable = queryable.Where(t => t.TripType == tripType.Value);
            if (!string.IsNullOrWhiteSpace(tripNumber))
                queryable = queryable.Where(t => t.TripNumber.Contains(tripNumber.Trim()));
            if (dateFrom.HasValue)
                queryable = queryable.Where(t => t.RecDate >= dateFrom.Value.ToUniversalTime());
            if (dateTo.HasValue)
                queryable = queryable.Where(t => t.RecDate <= dateTo.Value.Date.AddDays(1).ToUniversalTime());

            var trips = await queryable.OrderByDescending(t => t.RecDate).ToListAsync();

            var vm = new TripFilterVm
            {
                Status = status,
                TripType = tripType,
                DateFrom = dateFrom,
                DateTo = dateTo,
                TripNumber = tripNumber,
                Direction = direction,
                CurrentBranchId = cu.BranchId,
                Trips = trips.Select(t => new TripListItemVm
                {
                    Id = t.Id,
                    TripNumber = t.TripNumber,
                    TripType = t.TripType,
                    FromBranchName = t.FromBranch?.Name ?? "",
                    ToBranchName = t.ToBranch?.Name ?? "",
                    DriverName = t.Driver?.Name ?? "",
                    VehicleNumber = t.Vehicle?.VehicleNumber ?? "",
                    TripStatus = t.TripStatus,
                    TotalConsignments = t.TotalConsignments,
                    TotalWeight = t.TotalWeight,
                    ScheduledDeparture = t.ScheduledDeparture,
                    RecDate = t.RecDate,
                }).ToList()
            };
            return View(vm);
        }
        catch (Exception ex)
        {
            _notificationHelper.SetErrorMsg(ex.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Detail(long id)
    {
        try
        {
            var cu = await _currentUserProvider.GetCurrentUser();
            var trip = await _tripRepo.FindOrThrowAsync(id);

            var manifest = new List<TripManifestItemVm>();
            foreach (var tc in trip.TripConsignments)
            {
                var latest = await _statusLogRepo.GetLatestStatus(tc.ConsignmentId);
                manifest.Add(new TripManifestItemVm
                {
                    ConsignmentId = tc.ConsignmentId,
                    TrackingNumber = tc.Consignment?.TrackingNumber ?? "",
                    SenderName = tc.Consignment?.Sender?.Name ?? "",
                    ReceiverName = tc.Consignment?.Receiver?.Name ?? "",
                    DestinationBranchName = tc.Consignment?.DestinationBranch?.Name ?? "",
                    ChargeableWeight = tc.Consignment?.ChargeableWeight ?? 0,
                    PackageCount = tc.Consignment?.PackageCount ?? 0,
                    LoadedAt = tc.LoadedAt,
                    CurrentStatus = latest?.StatusType,
                    CurrentStatusLabel = latest?.StatusType.ToString(),
                });
            }

            var vm = new TripDetailVm
            {
                Id = trip.Id,
                TripNumber = trip.TripNumber,
                TripType = trip.TripType,
                FromBranchId = trip.FromBranchId,
                FromBranchName = trip.FromBranch?.Name ?? "",
                ToBranchId = trip.ToBranchId,
                ToBranchName = trip.ToBranch?.Name ?? "",
                DriverName = trip.Driver?.Name ?? "",
                DriverPhone = trip.Driver?.Phone ?? "",
                VehicleNumber = trip.Vehicle?.VehicleNumber ?? "",
                VehicleType = trip.Vehicle?.VehicleType.ToString() ?? "",
                TripStatus = trip.TripStatus,
                TotalConsignments = trip.TotalConsignments,
                TotalWeight = trip.TotalWeight,
                ScheduledDeparture = trip.ScheduledDeparture,
                ActualDeparture = trip.ActualDeparture,
                Notes = trip.Notes,
                RecDate = trip.RecDate,
                CurrentBranchId = cu.BranchId,
                Manifest = manifest,
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
    public async Task<IActionResult> Create([FromBody] TripCreateVm vm)
    {
        try
        {
            var cu = await _currentUserProvider.GetCurrentUser();
            var dto = new TripCreateDto
            {
                TripType = vm.TripType,
                FromBranchId = vm.FromBranchId > 0 ? vm.FromBranchId : cu.BranchId,
                ToBranchId = vm.ToBranchId > 0 ? vm.ToBranchId : (vm.TripType == TripType.Delivery ? cu.BranchId : vm.ToBranchId),
                DriverId = vm.DriverId,
                VehicleId = vm.VehicleId,
                ScheduledDeparture = vm.ScheduledDeparture,
                Notes = vm.Notes,
                ConsignmentIds = vm.ConsignmentIds,
            };
            var trip = await _tripService.Create(dto);
            return Json(new
            {
                success = true,
                message = $"Trip {trip.TripNumber} created with {vm.ConsignmentIds.Count} consignment(s).",
                tripId = trip.Id,
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Start(long id)
    {
        try
        {
            await _tripService.StartTrip(id);
            return Json(new { success = true, message = "Trip started — now in transit." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Complete(long id)
    {
        try
        {
            await _tripService.CompleteTrip(id);
            return Json(new { success = true, message = "Trip completed." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Cancel(long id)
    {
        try
        {
            await _tripService.CancelTrip(id);
            return Json(new { success = true, message = "Trip cancelled." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> ReceiveItem([FromBody] TripReceiveItemDto dto)
    {
        try
        {
            await _tripService.ReceiveTripItem(dto);
            return Json(new { success = true, message = $"Item marked {dto.Action}." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> MarkDelivered([FromBody] MarkDeliveredDto dto)
    {
        try
        {
            await _tripService.MarkDelivered(dto);
            return Json(new { success = true, message = "Marked delivered." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> MarkDeliveryFailed([FromBody] MarkDeliveryFailedDto dto)
    {
        try
        {
            await _tripService.MarkDeliveryFailed(dto);
            return Json(new { success = true, message = "Marked delivery failed." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}
