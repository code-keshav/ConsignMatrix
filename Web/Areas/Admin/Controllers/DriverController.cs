using Base.Constants;
using Base.Dtos;
using Base.Entities;
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
public class DriverController : Controller
{
    private readonly IDriverService _driverService;
    private readonly IDriverRepo _driverRepo;
    private readonly IBranchRepo _branchRepo;
    private readonly INotificationHelper _notificationHelper;
    private readonly ICurrentUserProvider _currentUserProvider;

    public DriverController(IDriverService driverService, IDriverRepo driverRepo,
        IBranchRepo branchRepo, INotificationHelper notificationHelper,
        ICurrentUserProvider currentUserProvider)
    {
        _driverService = driverService;
        _driverRepo = driverRepo;
        _branchRepo = branchRepo;
        _notificationHelper = notificationHelper;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string searchTerm, long? branchId, bool licenseExpiringSoon, int page = 1, int pageSize = 20)
    {
        var currentUser = await _currentUserProvider.GetCurrentUser();
        var isMainBranchUser = currentUser.BranchId == (long)IdConstants.MainBranchId;

        var query = _driverRepo.GetQueryable()
            .Include(d => d.Employee)
                .ThenInclude(e => e.CurrentBranch)
            .AsQueryable();

        // Branch filter
        if (!isMainBranchUser)
        {
            query = query.Where(d => d.Employee.CurrentBranchId == currentUser.BranchId);
        }
        else if (branchId.HasValue)
        {
            query = query.Where(d => d.Employee.CurrentBranchId == branchId.Value);
        }

        // Search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.Trim().ToLower();
            query = query.Where(d =>
                d.Employee.Name.ToLower().Contains(search) ||
                d.Employee.EmployeeCode.ToLower().Contains(search) ||
                d.Employee.Phone.Contains(search) ||
                d.LicenseNumber.ToLower().Contains(search));
        }

        // License expiring soon filter
        if (licenseExpiringSoon)
        {
            var threshold = DateTime.UtcNow.AddDays(30);
            query = query.Where(d => d.LicenseExpiry < threshold);
        }

        var totalCount = await query.CountAsync();

        var drivers = await query
            .OrderBy(d => d.Employee.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var driverVms = drivers.Select(d => new DriverListItemVm
        {
            Id = d.Id,
            EmployeeId = d.EmployeeId,
            EmployeeCode = d.Employee.EmployeeCode,
            EmployeeName = d.Employee.Name,
            BranchName = d.Employee.CurrentBranch?.Name,
            BranchCode = d.Employee.CurrentBranch?.Code,
            Phone = d.Employee.Phone,
            LicenseNumber = d.LicenseNumber,
            LicenseExpiry = d.LicenseExpiry
        }).ToList();

        var branches = isMainBranchUser
            ? await _branchRepo.GetQueryable().OrderBy(b => b.Name).ToListAsync()
            : new List<Branch>();

        var viewModel = new DriverIndexVm
        {
            Drivers = driverVms,
            Branches = branches,
            SearchTerm = searchTerm,
            BranchId = branchId,
            LicenseExpiringSoon = licenseExpiringSoon,
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            IsMainBranchUser = isMainBranchUser
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Update(long id)
    {
        try
        {
            var driver = await _driverRepo.GetQueryable()
                .Include(d => d.Employee)
                    .ThenInclude(e => e.CurrentBranch)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (driver == null)
                throw new Exception("Driver not found");

            var vm = new DriverEditVm
            {
                Id = driver.Id,
                EmployeeId = driver.EmployeeId,
                EmployeeName = driver.Employee.Name,
                EmployeeCode = driver.Employee.EmployeeCode,
                BranchName = driver.Employee.CurrentBranch?.Name,
                Phone = driver.Employee.Phone,
                LicenseNumber = driver.LicenseNumber,
                LicenseExpiry = driver.LicenseExpiry
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
    public async Task<IActionResult> Update(DriverEditVm vm)
    {
        try
        {
            var driver = await _driverRepo.GetQueryable()
                .Include(d => d.Employee)
                    .ThenInclude(e => e.CurrentBranch)
                .FirstOrDefaultAsync(d => d.Id == vm.Id);

            if (driver == null)
                throw new Exception("Driver not found");

            var dto = new DriverUpdateDto
            {
                LicenseNumber = vm.LicenseNumber,
                LicenseExpiry = vm.LicenseExpiry
            };

            await _driverService.Update(driver, dto);
            _notificationHelper.SetSuccessMsg("Driver updated successfully");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            await ReloadDriverEmployeeInfo(vm);
            _notificationHelper.SetErrorMsg(e.Message);
            return View(vm);
        }
    }

    private async Task ReloadDriverEmployeeInfo(DriverEditVm vm)
    {
        var driver = await _driverRepo.GetQueryable()
            .Include(d => d.Employee)
                .ThenInclude(e => e.CurrentBranch)
            .FirstOrDefaultAsync(d => d.Id == vm.Id);

        if (driver != null)
        {
            vm.EmployeeName = driver.Employee.Name;
            vm.EmployeeCode = driver.Employee.EmployeeCode;
            vm.BranchName = driver.Employee.CurrentBranch?.Name;
            vm.Phone = driver.Employee.Phone;
        }
    }
}
