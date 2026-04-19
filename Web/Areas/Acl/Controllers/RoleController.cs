using Base.Dtos;
using Base.Providers.Interfaces;
using Base.Repo.Interfaces;
using Base.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Areas.Acl.ViewModels;
using Web.Helpers;

namespace Web.Areas.Acl.Controllers;

[Area("Acl")]
public class RoleController : Controller
{
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IRoleService _roleService;
    private readonly INotificationHelper _notificationHelper;
    private readonly IRoleRepo _roleRepo;
    private readonly IBranchRepo _branchRepo;

    public RoleController(ICurrentUserProvider currentUserProvider, IRoleService roleService, INotificationHelper notificationHelper, IRoleRepo roleRepo,
        IBranchRepo branchRepo)
    {
        _currentUserProvider = currentUserProvider;
        _roleService = roleService;
        _notificationHelper = notificationHelper;
        _roleRepo = roleRepo;
        _branchRepo = branchRepo;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var roles = _roleRepo.GetQueryable();
        var report = await roles.Select(role => new RoleVm
        {
            Id = role.Id,
            Name = role.Name,
            Priority = role.Priority,
            BranchCode = role.Branch.Code,
            BranchName = role.Branch.Name,
            IsGlobal = role.IsGlobal
        }).ToListAsync();
        return View(report);
    }


    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(RoleVm vm)
    {
        try
        {
            var branch = await _currentUserProvider.GetUserBranch();
            var dto = new RoleDto
            {
                Name = vm.Name,
                Priority = vm.Priority,
                Branch = branch,
                IsGlobal = vm.IsGlobal,
            };
            await _roleService.Create(dto);
            _notificationHelper.SetSuccessMsg("Role created successfully");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return RedirectToAction(nameof(Create));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Update(long id)
    {
        try
        {
            var role = await _roleRepo.FindOrThrowAsync(id);
            var vm = new RoleEditVm
            {
                Id = role.Id,
                Name = role.Name,
                Priority = role.Priority
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
    public async Task<IActionResult> Update(RoleEditVm vm)
    {
        try
        {
            var role = await _roleRepo.FindOrThrowAsync(vm.Id);
            var dto = new RoleEditDto
            {
                Name = vm.Name,
                Priority = vm.Priority
            };
            await _roleService.Update(role, dto);
            _notificationHelper.SetSuccessMsg("Role updated successfully");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return RedirectToAction(nameof(Update));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            var role = await _roleRepo.FindOrThrowAsync(id);
            await _roleService.Delete(role);
            _notificationHelper.SetSuccessMsg("Role deleted successfully");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> MarkAsGlobal(long id)
    {
        try
        {
            var branch = await _branchRepo.GetMainBranch();
            var role = await _roleRepo.FindOrThrowAsync(id);
            await _roleService.MarkAsGlobal(role, branch);
            _notificationHelper.SetSuccessMsg("Role marked as global successfully");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> UnmarkAsGlobal(long id)
    {
        try
        {
            var branch = await _currentUserProvider.GetUserBranch();
            var role = await _roleRepo.FindOrThrowAsync(id);
            await _roleService.MarkAsGlobal(role, branch);
            _notificationHelper.SetSuccessMsg("Role unmarked as global successfully");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return RedirectToAction(nameof(Index));
        }
    }
}