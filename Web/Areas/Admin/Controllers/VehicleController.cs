using Base.Constants;
using Base.Dtos;
using Base.Entities;
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
public class VehicleController : Controller
{
    private readonly IVehicleService _vehicleService;
    private readonly IVehicleRepo _vehicleRepo;
    private readonly IBranchRepo _branchRepo;
    private readonly INotificationHelper _notificationHelper;
    private readonly ICurrentUserProvider _currentUserProvider;

    public VehicleController(IVehicleService vehicleService, IVehicleRepo vehicleRepo,
        IBranchRepo branchRepo, INotificationHelper notificationHelper,
        ICurrentUserProvider currentUserProvider)
    {
        _vehicleService = vehicleService;
        _vehicleRepo = vehicleRepo;
        _branchRepo = branchRepo;
        _notificationHelper = notificationHelper;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string searchTerm, long? branchId, int? vehicleType, int? vehicleStatus, int page = 1, int pageSize = 20)
    {
        var currentUser = await _currentUserProvider.GetCurrentUser();
        var isMainBranchUser = currentUser.BranchId == (long)IdConstants.MainBranchId;

        var query = _vehicleRepo.GetQueryable()
            .Include(x => x.CurrentBranch)
            .AsQueryable();

        // Branch filter
        if (!isMainBranchUser)
        {
            query = query.Where(x => x.CurrentBranchId == currentUser.BranchId);
        }
        else if (branchId.HasValue)
        {
            query = query.Where(x => x.CurrentBranchId == branchId.Value);
        }

        // Search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.Trim().ToLower();
            query = query.Where(x =>
                x.VehicleNumber.ToLower().Contains(search));
        }

        // VehicleType filter
        if (vehicleType.HasValue)
        {
            query = query.Where(x => (int)x.VehicleType == vehicleType.Value);
        }

        // VehicleStatus filter
        if (vehicleStatus.HasValue)
        {
            query = query.Where(x => (int)x.VehicleStatus == vehicleStatus.Value);
        }

        var totalCount = await query.CountAsync();

        var vehicles = await query
            .OrderBy(x => x.VehicleNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var vehicleVms = vehicles.Select(v => new VehicleListItemVm
        {
            Id = v.Id,
            VehicleNumber = v.VehicleNumber,
            VehicleType = v.VehicleType,
            OwnershipType = v.OwnershipType,
            MaxWeightCapacity = v.MaxWeightCapacity,
            MaxVolumeCapacity = v.MaxVolumeCapacity,
            VehicleStatus = v.VehicleStatus,
            InsuranceExpiry = v.InsuranceExpiry,
            FuelType = v.FuelType,
            BranchName = v.CurrentBranch?.Name,
            BranchCode = v.CurrentBranch?.Code
        }).ToList();

        var branches = isMainBranchUser
            ? await _branchRepo.GetQueryable().OrderBy(b => b.Name).ToListAsync()
            : new List<Branch>();

        var viewModel = new VehicleIndexVm
        {
            Vehicles = vehicleVms,
            Branches = branches,
            SearchTerm = searchTerm,
            BranchId = branchId,
            VehicleType = vehicleType,
            VehicleStatus = vehicleStatus,
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            IsMainBranchUser = isMainBranchUser
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new VehicleCreateVm();
        var userBranchId = await _currentUserProvider.GetUserBranchId();
        vm.CurrentBranchId = userBranchId;
        vm.Branches = await _branchRepo.GetQueryable()
            .Where(x => userBranchId == (long)IdConstants.MainBranchId || x.Id == userBranchId)
            .ToListAsync();
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Create(VehicleCreateVm vm)
    {
        try
        {
            var userBranchId = await _currentUserProvider.GetUserBranchId();
            vm.Branches = await _branchRepo.GetQueryable()
                .Where(x => userBranchId == (long)IdConstants.MainBranchId || x.Id == userBranchId)
                .ToListAsync();

            var dto = new VehicleAddDto
            {
                VehicleNumber = vm.VehicleNumber,
                VehicleType = vm.VehicleType,
                OwnershipType = vm.OwnershipType,
                MaxWeightCapacity = vm.MaxWeightCapacity,
                MaxVolumeCapacity = vm.MaxVolumeCapacity,
                SupportsFragile = vm.SupportsFragile,
                HasColdStorage = vm.HasColdStorage,
                VehicleStatus = vm.VehicleStatus,
                LastServiceDate = vm.LastServiceDate,
                InsuranceExpiry = vm.InsuranceExpiry,
                FuelType = vm.FuelType,
                CurrentBranchId = userBranchId == (long)IdConstants.MainBranchId ? vm.CurrentBranchId : userBranchId
            };

            await _vehicleService.Create(dto);
            _notificationHelper.SetSuccessMsg("Vehicle created successfully");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return View(vm);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Update(long id)
    {
        try
        {
            var vehicle = await _vehicleRepo.FindOrThrowAsync(id);
            var userBranchId = await _currentUserProvider.GetUserBranchId();

            var vm = new VehicleEditVm
            {
                Id = vehicle.Id,
                VehicleNumber = vehicle.VehicleNumber,
                VehicleType = vehicle.VehicleType,
                OwnershipType = vehicle.OwnershipType,
                MaxWeightCapacity = vehicle.MaxWeightCapacity,
                MaxVolumeCapacity = vehicle.MaxVolumeCapacity,
                SupportsFragile = vehicle.SupportsFragile,
                HasColdStorage = vehicle.HasColdStorage,
                VehicleStatus = vehicle.VehicleStatus,
                LastServiceDate = vehicle.LastServiceDate,
                InsuranceExpiry = vehicle.InsuranceExpiry,
                FuelType = vehicle.FuelType,
                CurrentBranchId = vehicle.CurrentBranchId,
                Branches = await _branchRepo.GetQueryable()
                    .Where(x => userBranchId == (long)IdConstants.MainBranchId || x.Id == userBranchId)
                    .ToListAsync()
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
    public async Task<IActionResult> Update(VehicleEditVm vm)
    {
        try
        {
            var userBranchId = await _currentUserProvider.GetUserBranchId();
            vm.Branches = await _branchRepo.GetQueryable()
                .Where(x => userBranchId == (long)IdConstants.MainBranchId || x.Id == userBranchId)
                .ToListAsync();

            var vehicle = await _vehicleRepo.FindOrThrowAsync(vm.Id);

            var dto = new VehicleUpdateDto
            {
                VehicleNumber = vm.VehicleNumber,
                VehicleType = vm.VehicleType,
                OwnershipType = vm.OwnershipType,
                MaxWeightCapacity = vm.MaxWeightCapacity,
                MaxVolumeCapacity = vm.MaxVolumeCapacity,
                SupportsFragile = vm.SupportsFragile,
                HasColdStorage = vm.HasColdStorage,
                VehicleStatus = vm.VehicleStatus,
                LastServiceDate = vm.LastServiceDate,
                InsuranceExpiry = vm.InsuranceExpiry,
                FuelType = vm.FuelType,
                CurrentBranchId = userBranchId == (long)IdConstants.MainBranchId ? vm.CurrentBranchId : userBranchId
            };

            await _vehicleService.Update(vehicle, dto);
            _notificationHelper.SetSuccessMsg("Vehicle updated successfully");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return View(vm);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            var vehicle = await _vehicleRepo.FindOrThrowAsync(id);
            await _vehicleService.Delete(vehicle);
            _notificationHelper.SetSuccessMsg("Vehicle deleted successfully");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return RedirectToAction(nameof(Index));
        }
    }
}
