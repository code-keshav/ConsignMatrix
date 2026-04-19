using Base.Constants;
using Base.Dtos;
using Base.Entities;
using Base.Enum;
using Base.Manager.Interface;
using Base.Providers.Interfaces;
using Base.Repo.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Areas.Admin.ViewModels;
using Web.Helpers;

namespace Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("[area]/[controller]/[action]")]
public class EmployeeController : Controller
{
    private readonly IEmployeeManager _employeeManager;
    private readonly IEmployeeRepo _employeeRepo;
    private readonly IDriverRepo _driverRepo;
    private readonly IBranchRepo _branchRepo;
    private readonly INotificationHelper _notificationHelper;
    private readonly ICurrentUserProvider _currentUserProvider;

    public EmployeeController(IEmployeeManager employeeManager, IEmployeeRepo employeeRepo,
        IDriverRepo driverRepo, IBranchRepo branchRepo,
        INotificationHelper notificationHelper, ICurrentUserProvider currentUserProvider)
    {
        _employeeManager = employeeManager;
        _employeeRepo = employeeRepo;
        _driverRepo = driverRepo;
        _branchRepo = branchRepo;
        _notificationHelper = notificationHelper;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string searchTerm, long? branchId, int? employeeType, int? employeeStatus, int page = 1, int pageSize = 20)
    {
        var currentUser = await _currentUserProvider.GetCurrentUser();
        var isMainBranchUser = currentUser.BranchId == (long)IdConstants.MainBranchId;

        var query = _employeeRepo.GetQueryable()
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
                x.Name.ToLower().Contains(search) ||
                x.EmployeeCode.ToLower().Contains(search) ||
                x.Phone.Contains(search) ||
                (x.Email != null && x.Email.ToLower().Contains(search)));
        }

        // EmployeeType filter
        if (employeeType.HasValue)
        {
            query = query.Where(x => (int)x.EmployeeType == employeeType.Value);
        }

        // EmployeeStatus filter
        if (employeeStatus.HasValue)
        {
            query = query.Where(x => (int)x.EmployeeStatus == employeeStatus.Value);
        }

        var totalCount = await query.CountAsync();

        var employees = await query
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Load driver IDs for driver-type employees
        var employeeIds = employees.Where(e => e.EmployeeType == EmployeeType.Driver).Select(e => e.Id).ToList();
        var driverMap = employeeIds.Any()
            ? await _driverRepo.GetQueryable()
                .Where(d => employeeIds.Contains(d.EmployeeId))
                .ToDictionaryAsync(d => d.EmployeeId, d => d.Id)
            : new Dictionary<long, long>();

        var employeeVms = employees.Select(e => new EmployeeListItemVm
        {
            Id = e.Id,
            EmployeeCode = e.EmployeeCode,
            Name = e.Name,
            Phone = e.Phone,
            Email = e.Email,
            EmployeeType = e.EmployeeType,
            EmployeeStatus = e.EmployeeStatus,
            BranchName = e.CurrentBranch.Name,
            BranchCode = e.CurrentBranch.Code,
            HasLogin = e.UserId.HasValue,
            Department = e.Department,
            Designation = e.Designation,
            DriverId = driverMap.GetValueOrDefault(e.Id)
        }).ToList();

        var branches = isMainBranchUser
            ? await _branchRepo.GetQueryable().OrderBy(b => b.Name).ToListAsync()
            : new List<Branch>();

        var viewModel = new EmployeeIndexVm
        {
            Employees = employeeVms,
            Branches = branches,
            SearchTerm = searchTerm,
            BranchId = branchId,
            EmployeeType = employeeType,
            EmployeeStatus = employeeStatus,
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
        var vm = new EmployeeCreateVm();
        var userBranchId = await _currentUserProvider.GetUserBranchId();
        vm.CurrentBranchId = userBranchId;
        vm.Branches = await _branchRepo.GetQueryable()
            .Where(x => userBranchId == (long)IdConstants.MainBranchId || x.Id == userBranchId)
            .ToListAsync();
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Create(EmployeeCreateVm vm)
    {
        try
        {
            var userBranchId = await _currentUserProvider.GetUserBranchId();
            vm.Branches = await _branchRepo.GetQueryable()
                .Where(x => userBranchId == (long)IdConstants.MainBranchId || x.Id == userBranchId)
                .ToListAsync();

            var dto = new EmployeeAddDto
            {
                EmployeeCode = vm.EmployeeCode,
                Name = vm.Name,
                Phone = vm.Phone,
                AlternatePhone = vm.AlternatePhone,
                Email = vm.Email,
                Address = vm.Address,
                EmployeeType = vm.EmployeeType,
                EmployeeStatus = vm.EmployeeStatus,
                JoiningDate = vm.JoiningDate,
                Department = vm.Department,
                Designation = vm.Designation,
                CurrentBranchId = userBranchId == (long)IdConstants.MainBranchId ? vm.CurrentBranchId : userBranchId,
                CreateUserAccount = vm.CreateUserAccount,
                Password = vm.Password,
                ConfirmPassword = vm.ConfirmPassword,
                LicenseNumber = vm.LicenseNumber,
                LicenseExpiry = vm.LicenseExpiry,
                Username = vm.Username
            };

            await _employeeManager.Create(dto);
            _notificationHelper.SetSuccessMsg("Employee created successfully");
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
            var employee = await _employeeRepo.FindOrThrowAsync(id);
            var userBranchId = await _currentUserProvider.GetUserBranchId();

            var vm = new EmployeeEditVm
            {
                Id = employee.Id,
                EmployeeCode = employee.EmployeeCode,
                Name = employee.Name,
                Phone = employee.Phone,
                AlternatePhone = employee.AlternatePhone,
                Email = employee.Email,
                Address = employee.Address,
                EmployeeType = employee.EmployeeType,
                EmployeeStatus = employee.EmployeeStatus,
                JoiningDate = employee.JoiningDate,
                TerminationDate = employee.TerminationDate,
                Department = employee.Department,
                Designation = employee.Designation,
                CurrentBranchId = employee.CurrentBranchId,
                HasUserAccount = employee.UserId.HasValue,
                Branches = await _branchRepo.GetQueryable()
                    .Where(x => userBranchId == (long)IdConstants.MainBranchId || x.Id == userBranchId)
                    .ToListAsync()
            };

            // Load driver data if exists
            var driver = await _driverRepo.FindSingleAsync(d => d.EmployeeId == id);
            if (driver != null)
            {
                vm.LicenseNumber = driver.LicenseNumber;
                vm.LicenseExpiry = driver.LicenseExpiry;
                vm.DriverId = driver.Id;
            }

            return View(vm);
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Update(EmployeeEditVm vm)
    {
        try
        {
            var userBranchId = await _currentUserProvider.GetUserBranchId();
            vm.Branches = await _branchRepo.GetQueryable()
                .Where(x => userBranchId == (long)IdConstants.MainBranchId || x.Id == userBranchId)
                .ToListAsync();

            var dto = new EmployeeUpdateDto
            {
                EmployeeCode = vm.EmployeeCode,
                Name = vm.Name,
                Phone = vm.Phone,
                AlternatePhone = vm.AlternatePhone,
                Email = vm.Email,
                Address = vm.Address,
                EmployeeType = vm.EmployeeType,
                EmployeeStatus = vm.EmployeeStatus,
                JoiningDate = vm.JoiningDate,
                TerminationDate = vm.TerminationDate,
                Department = vm.Department,
                Designation = vm.Designation,
                CurrentBranchId = userBranchId == (long)IdConstants.MainBranchId ? vm.CurrentBranchId : userBranchId,
                LicenseNumber = vm.LicenseNumber,
                LicenseExpiry = vm.LicenseExpiry
            };

            await _employeeManager.Update(vm.Id, dto);
            _notificationHelper.SetSuccessMsg("Employee updated successfully");
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
            await _employeeManager.Delete(id);
            _notificationHelper.SetSuccessMsg("Employee deleted successfully");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return RedirectToAction(nameof(Index));
        }
    }
}
