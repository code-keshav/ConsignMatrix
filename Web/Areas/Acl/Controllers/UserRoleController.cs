using Base.Dtos;
using Base.Providers.Interfaces;
using Base.Repo.Interfaces;
using Base.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Web.Areas.Acl.ViewModels;
using Web.Helpers;

namespace Web.Areas.Acl.Controllers;

[Area("Acl")]
public class UserRoleController : Controller
{
    private readonly INotificationHelper _notificationHelper;
    private readonly IUserRoleService _userRoleService;
    private readonly IRoleRepo _roleRepo;
    private readonly IUserRepo _userRepo;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IUserRoleRepo _userRoleRepo;

    public UserRoleController(INotificationHelper notificationHelper, IUserRoleService userRoleService, IRoleRepo roleRepo, IUserRepo userRepo,
        ICurrentUserProvider currentUserProvider,
        IUserRoleRepo userRoleRepo)
    {
        _notificationHelper = notificationHelper;
        _userRoleService = userRoleService;
        _roleRepo = roleRepo;
        _userRepo = userRepo;
        _currentUserProvider = currentUserProvider;
        _userRoleRepo = userRoleRepo;
    }

    [HttpPost]
    public async Task<IActionResult> AssignRole(UserRoleUpdateVm vm)
    {
        try
        {
            var dto = new UserRoleDto
            {
                User = await _userRepo.FindOrThrowAsync(vm.UserId),
                Roles = await _roleRepo.FindByAsync(a => vm.RoleIds.Contains(a.Id)),
                Branch = await _currentUserProvider.GetUserBranch()
            };
            await _userRoleService.AssignRole(dto);
            _notificationHelper.SetSuccessMsg("Role assigned successfully");
            return RedirectToAction(actionName: "Index", controllerName: "User", routeValues: new { area = "Admin" });
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return RedirectToAction(actionName: "Index", controllerName: "User", routeValues: new { area = "Admin" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> UnassignRole(long userId)
    {
        try
        {
            var userRole = await _userRoleRepo.FindSingleOrThrowAsync(a => a.UserId == userId);
            await _userRoleService.UnassignRole(userRole);
            _notificationHelper.SetSuccessMsg("Role unassigned successfully");
            return RedirectToAction(actionName: "Index", controllerName: "User", routeValues: new { area = "Admin" });
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return RedirectToAction(actionName: "Index", controllerName: "User", routeValues: new { area = "Admin" });
        }
    }
}