using Acl.Dtos;
using Acl.Helper.Interface;
using Acl.Repo.Interfaces;
using Acl.Services.Interfaces;
using Base.Constants;
using Base.Providers.Interfaces;
using Base.Repo.Interfaces;
using Base.Validator.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Areas.Acl.ViewModels;
using Web.Extensions;
using Web.Helpers;

namespace Web.Areas.Acl.Controllers;

[Area("Acl")]
public class RolePermissionController : Controller
{
    private readonly ICurrentUserProvider _userProvider;
    private readonly IBranchRepo _branchRepo;
    private readonly IRoleRepo _roleRepo;
    private readonly IRolePermissionService _rolePermissionService;
    private readonly INotificationHelper _notificationHelper;
    private readonly IRoleValidator _roleValidator;
    private readonly IRolePermissionRepo _rolePermissionRepo;
    private readonly IPermissionChecker _permissionChecker;
    private readonly IPermissionProvider _permissionProvider;

    public RolePermissionController(ICurrentUserProvider userProvider, IBranchRepo branchRepo, IRoleRepo roleRepo, IRolePermissionService rolePermissionService,
        INotificationHelper notificationHelper, IRoleValidator roleValidator, IRolePermissionRepo rolePermissionRepo, IPermissionChecker permissionChecker, IPermissionProvider permissionProvider)
    {
        _userProvider = userProvider;
        _branchRepo = branchRepo;
        _roleRepo = roleRepo;
        _rolePermissionService = rolePermissionService;
        _notificationHelper = notificationHelper;
        _roleValidator = roleValidator;
        _rolePermissionRepo = rolePermissionRepo;
        _permissionChecker = permissionChecker;
        _permissionProvider = permissionProvider;
    }

    [HttpGet]
    public async Task<IActionResult> Index(long? roleId)
    {
        try
        {
            var branchId = await _userProvider.GetUserBranchId();
            var roles = await _roleRepo.GetQueryable().Where(a => a.BranchId == branchId || a.IsGlobal).ToListAsync();
            var allPermissions = _permissionProvider.GetPermissionTree();

            if (roleId != null)
            {
                var role = await _roleRepo.FindOrThrowAsync((long)roleId);
                await _roleValidator.ValidateRoleUpdate(role);
                var permissions =
                    await _rolePermissionRepo.GetPermissions((long)roleId, role.IsGlobal ? (long)IdConstants.MainBranchId : branchId);
                var vm = new RolePermissionUpdateVm
                {
                    RoleId = roleId,
                    RoleName = role.Name,
                    Permissions = permissions,
                    Roles = roles,
                    Role = role,
                    AllPermissions = allPermissions
                };
                return View(vm);
            }

            return View(new RolePermissionUpdateVm
            {
                Roles = roles,
                AllPermissions = allPermissions
            });
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return RedirectToAction("Index", "Role");
        }
    }

    [HttpPost]
    public async Task<IActionResult> SavePermissions(long roleId, List<string> selectedPermissions)
    {
        try
        {
            var currentBranch = await _userProvider.GetUserBranch();
            var branches = await _branchRepo.GetQueryable().ToListAsync();
            if (selectedPermissions.Count > 0)
            {
                var role = await  _roleRepo.FindOrThrowAsync(roleId);
                if (!role.IsGlobal) await _userProvider.ValidateBranchUsage(role.BranchId);
                else if (!await _userProvider.IsMainBranch()) throw new Exception("Error: Access Denied. You cannot edit a permission of global role");
                
                var rolePermissionDtos = new List<RolePermissionDto>();
                if (role.IsGlobal)
                {
                    foreach (var branch in branches)
                    {
                        var rolePermissionDto = new RolePermissionDto
                        {
                            Branch = branch,
                            Role = role,
                            Permissions = selectedPermissions ?? new List<string>(),
                        };
                        rolePermissionDtos.Add(rolePermissionDto);
                    }
                }
                else
                {
                    var dto = new RolePermissionDto
                    {
                        Branch = currentBranch,
                        Role = role,
                        Permissions = selectedPermissions,
                    };
                    rolePermissionDtos.Add(dto);
                }
                await _rolePermissionService.UpdatePermission(rolePermissionDtos);
            }

            _notificationHelper.SetSuccessMsg("Permission updated successfully");
            return RedirectToAction("Index", "RolePermission", new { roleId });
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return RedirectToAction("Index", "Role");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUserAclEndpoints()
    {
        try
        {
            var endpoints = _permissionProvider.GetLeafPermissionsValue();
            return this.SendSuccess("", endpoints);
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return RedirectToAction("Index", "Role");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetUserAcl()
    {
        try
        {
            var user = await _userProvider.GetCurrentUser();
            return this.SendSuccess(null, await _permissionChecker.GetUserPermissionsAsync(user));
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return RedirectToAction("Index", "Role");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAclDict()
    {
        try
        {
            var result = _permissionProvider.GetLeafPermissionsDictionary();
            return this.SendSuccess("", result);
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return RedirectToAction("Index", "Role");
        }
    }
}