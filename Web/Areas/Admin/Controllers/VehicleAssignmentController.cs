using Base.Constants;
using Base.Dtos;
using Base.Enum;
using Base.Providers.Interfaces;
using Base.Repo.Interfaces;
using Base.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Areas.Admin.ViewModels;
using Web.Helpers;

namespace Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("[area]/[controller]/[action]")]
public class VehicleAssignmentController : Controller
{
    private readonly IVehicleAssignmentService _vehicleAssignmentService;
    private readonly IVehicleAssignmentRepo _vehicleAssignmentRepo;
    private readonly IVehicleRepo _vehicleRepo;
    private readonly IEmployeeRepo _employeeRepo;
    private readonly INotificationHelper _notificationHelper;
    private readonly ICurrentUserProvider _currentUserProvider;

    public VehicleAssignmentController(
        IVehicleAssignmentService vehicleAssignmentService,
        IVehicleAssignmentRepo vehicleAssignmentRepo,
        IVehicleRepo vehicleRepo,
        IEmployeeRepo employeeRepo,
        INotificationHelper notificationHelper,
        ICurrentUserProvider currentUserProvider)
    {
        _vehicleAssignmentService = vehicleAssignmentService;
        _vehicleAssignmentRepo = vehicleAssignmentRepo;
        _vehicleRepo = vehicleRepo;
        _employeeRepo = employeeRepo;
        _notificationHelper = notificationHelper;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet]
    public async Task<IActionResult> Index(long? vehicleId, long? employeeId, int? assignmentType, bool? isActive = true, int page = 1, int pageSize = 20)
    {
        var currentUser = await _currentUserProvider.GetCurrentUser();
        var isMainBranchUser = currentUser.BranchId == (long)IdConstants.MainBranchId;

        var query = _vehicleAssignmentRepo.GetQueryable()
            .Include(x => x.Vehicle)
            .Include(x => x.Employee)
            .AsQueryable();

        if (!isMainBranchUser)
        {
            query = query.Where(x => x.Vehicle.CurrentBranchId == currentUser.BranchId);
        }

        if (vehicleId.HasValue)
            query = query.Where(x => x.VehicleId == vehicleId.Value);

        if (employeeId.HasValue)
            query = query.Where(x => x.EmployeeId == employeeId.Value);

        if (assignmentType.HasValue)
            query = query.Where(x => (int)x.AssignmentType == assignmentType.Value);

        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        var totalCount = await query.CountAsync();

        var assignments = await query
            .OrderByDescending(x => x.AssignedFrom)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var vms = assignments.Select(a => new VehicleAssignmentListItemVm
        {
            Id = a.Id,
            VehicleNumber = a.Vehicle.VehicleNumber,
            VehicleId = a.VehicleId,
            EmployeeName = a.Employee.Name,
            EmployeeCode = a.Employee.EmployeeCode,
            AssignmentType = a.AssignmentType,
            AssignedFrom = a.AssignedFrom,
            AssignedTo = a.AssignedTo,
            IsActive = a.IsActive
        }).ToList();

        var vehicles = isMainBranchUser
            ? await _vehicleRepo.GetQueryable().OrderBy(v => v.VehicleNumber).ToListAsync()
            : await _vehicleRepo.GetQueryable().Where(v => v.CurrentBranchId == currentUser.BranchId).OrderBy(v => v.VehicleNumber).ToListAsync();

        var viewModel = new VehicleAssignmentIndexVm
        {
            Assignments = vms,
            Vehicles = vehicles,
            VehicleId = vehicleId,
            EmployeeId = employeeId,
            AssignmentType = assignmentType,
            IsActive = isActive,
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create(long? vehicleId)
    {
        var currentUser = await _currentUserProvider.GetCurrentUser();
        var isMainBranchUser = currentUser.BranchId == (long)IdConstants.MainBranchId;

        var vm = new VehicleAssignmentCreateVm();

        if (vehicleId.HasValue)
        {
            var vehicle = await _vehicleRepo.FindOrThrowAsync(vehicleId.Value);
            vm.VehicleId = vehicle.Id;
            vm.VehicleNumber = vehicle.VehicleNumber;
            vm.IsVehicleReadonly = true;
        }

        vm.Vehicles = isMainBranchUser
            ? await _vehicleRepo.GetQueryable()
                .Where(v => v.VehicleStatus != VehicleStatus.Inactive)
                .OrderBy(v => v.VehicleNumber).ToListAsync()
            : await _vehicleRepo.GetQueryable()
                .Where(v => v.VehicleStatus != VehicleStatus.Inactive && v.CurrentBranchId == currentUser.BranchId)
                .OrderBy(v => v.VehicleNumber).ToListAsync();

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Create(VehicleAssignmentCreateVm vm)
    {
        try
        {
            if (vm.VehicleId == null || vm.VehicleId == 0)
                throw new Exception("Please select a vehicle");

            var rows = vm.Rows?.Where(r => r.EmployeeId > 0).ToList() ?? new();
            if (!rows.Any())
                throw new Exception("Please add at least one assignment row");

            var dtos = rows.Select(r => new VehicleAssignmentAddDto
            {
                VehicleId = vm.VehicleId.Value,
                EmployeeId = r.EmployeeId,
                AssignmentType = r.AssignmentType,
                AssignedFrom = r.AssignedFrom,
                AssignedTo = r.AssignedTo
            }).ToList();

            await _vehicleAssignmentService.CreateBulk(vm.VehicleId.Value, dtos);
            _notificationHelper.SetSuccessMsg("Vehicle assignments created successfully");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);

            var currentUser = await _currentUserProvider.GetCurrentUser();
            var isMainBranchUser = currentUser.BranchId == (long)IdConstants.MainBranchId;
            vm.Vehicles = isMainBranchUser
                ? await _vehicleRepo.GetQueryable()
                    .Where(v => v.VehicleStatus != VehicleStatus.Inactive)
                    .OrderBy(v => v.VehicleNumber).ToListAsync()
                : await _vehicleRepo.GetQueryable()
                    .Where(v => v.VehicleStatus != VehicleStatus.Inactive && v.CurrentBranchId == currentUser.BranchId)
                    .OrderBy(v => v.VehicleNumber).ToListAsync();

            if (vm.VehicleId.HasValue)
            {
                var vehicle = await _vehicleRepo.FindByIdAsync(vm.VehicleId.Value);
                if (vehicle != null) vm.VehicleNumber = vehicle.VehicleNumber;
            }

            return View(vm);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Update(long vehicleId)
    {
        try
        {
            var vehicle = await _vehicleRepo.FindOrThrowAsync(vehicleId);

            var assignments = await _vehicleAssignmentRepo.GetQueryable()
                .Include(a => a.Employee)
                .Where(a => a.VehicleId == vehicleId)
                .OrderByDescending(a => a.IsActive)
                .ThenByDescending(a => a.AssignedFrom)
                .ToListAsync();

            var vm = new VehicleAssignmentEditVm
            {
                VehicleId = vehicle.Id,
                VehicleNumber = vehicle.VehicleNumber,
                Rows = assignments.Select(a => new VehicleAssignmentEditRowVm
                {
                    Id = a.Id,
                    EmployeeId = a.EmployeeId,
                    EmployeeName = a.Employee.Name,
                    EmployeeCode = a.Employee.EmployeeCode,
                    AssignmentType = a.AssignmentType,
                    AssignedFrom = a.AssignedFrom,
                    AssignedTo = a.AssignedTo,
                    IsActive = a.IsActive
                }).ToList()
            };

            return View(vm);
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Update(VehicleAssignmentEditVm vm)
    {
        try
        {
            foreach (var row in vm.Rows)
            {
                var assignment = await _vehicleAssignmentRepo.FindOrThrowAsync(row.Id);

                var dto = new VehicleAssignmentUpdateDto
                {
                    AssignmentType = row.AssignmentType,
                    AssignedFrom = row.AssignedFrom,
                    AssignedTo = row.AssignedTo,
                    IsActive = row.IsActive
                };

                await _vehicleAssignmentService.Update(assignment, dto);
            }

            _notificationHelper.SetSuccessMsg("Vehicle assignments updated successfully");
            return RedirectToAction(nameof(Update), new { vehicleId = vm.VehicleId });
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return RedirectToAction(nameof(Update), new { vehicleId = vm.VehicleId });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Remove(long id)
    {
        try
        {
            var assignment = await _vehicleAssignmentRepo.FindOrThrowAsync(id);
            await _vehicleAssignmentService.Delete(assignment);
            _notificationHelper.SetSuccessMsg("Assignment deleted successfully");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Deactivate(long id)
    {
        try
        {
            var assignment = await _vehicleAssignmentRepo.FindOrThrowAsync(id);
            await _vehicleAssignmentService.Deactivate(assignment);
            _notificationHelper.SetSuccessMsg("Assignment deactivated successfully");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetEmployeesByVehicle(long vehicleId)
    {
        var vehicle = await _vehicleRepo.FindByIdAsync(vehicleId);
        if (vehicle == null)
            return Json(new List<object>());

        var employees = await _employeeRepo.GetQueryable()
            .Where(e => e.CurrentBranchId == vehicle.CurrentBranchId &&
                        e.EmployeeStatus == EmployeeStatus.Active)
            .OrderBy(e => e.Name)
            .Select(e => new
            {
                id = e.Id,
                name = e.Name,
                code = e.EmployeeCode,
                employeeType = (int)e.EmployeeType
            })
            .ToListAsync();

        return Json(employees);
    }
}
