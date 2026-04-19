using Base.Dtos;
using Base.Repo.Interfaces;
using Base.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Areas.Admin.Responses;
using Web.Areas.Admin.ViewModels;
using Web.Helpers;

namespace Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("[area]/[controller]/[action]")]
public class BranchPinCodeController : Controller
{
    private readonly IBranchPinCodeRepo _pinCodeRepo;
    private readonly IBranchPinCodeService _pinCodeService;
    private readonly IBranchRepo _branchRepo;
    private readonly INotificationHelper _notificationHelper;

    public BranchPinCodeController(IBranchPinCodeRepo pinCodeRepo, IBranchPinCodeService pinCodeService,
        IBranchRepo branchRepo, INotificationHelper notificationHelper)
    {
        _pinCodeRepo = pinCodeRepo;
        _pinCodeService = pinCodeService;
        _branchRepo = branchRepo;
        _notificationHelper = notificationHelper;
    }

    [HttpGet]
    public async Task<IActionResult> Report(long? branchId)
    {
        try
        {
            var query = _pinCodeRepo.GetQueryable().AsQueryable();

            if (branchId.HasValue)
                query = query.Where(p => p.BranchId == branchId.Value);

            var list = await query.Select(p => new BranchPinCodeReportResponse
            {
                Id = p.Id,
                BranchId = p.BranchId,
                BranchName = p.Branch.Name,
                PinCode = p.PinCode,
                IsActive = p.IsActive
            }).ToListAsync();

            ViewBag.BranchId = branchId;
            ViewBag.Branches = await _branchRepo.GetQueryable()
                .Select(b => new { b.Id, b.Name })
                .ToListAsync();

            return View(list);
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return RedirectToAction(nameof(Report));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Branches = await _branchRepo.GetQueryable()
            .Select(b => new { b.Id, b.Name })
            .ToListAsync();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(BranchPinCodeVm vm)
    {
        try
        {
            var dto = new BranchPinCodeDto
            {
                BranchId = vm.BranchId,
                PinCode = vm.PinCode,
                IsActive = vm.IsActive
            };
            await _pinCodeService.Create(dto);
            _notificationHelper.SetSuccessMsg("Pin Code added successfully");
            return RedirectToAction(nameof(Report));
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            ViewBag.Branches = await _branchRepo.GetQueryable()
                .Select(b => new { b.Id, b.Name })
                .ToListAsync();
            return View(vm);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Update(long id)
    {
        try
        {
            var pinCode = await _pinCodeRepo.FindOrThrowAsync(id);
            var vm = new BranchPinCodeVm
            {
                Id = pinCode.Id,
                BranchId = pinCode.BranchId,
                PinCode = pinCode.PinCode,
                IsActive = pinCode.IsActive
            };
            ViewBag.BranchName = (await _branchRepo.FindOrThrowAsync(pinCode.BranchId)).Name;
            return View(vm);
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return RedirectToAction(nameof(Report));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Update(BranchPinCodeVm vm)
    {
        try
        {
            var pinCode = await _pinCodeRepo.FindOrThrowAsync(vm.Id);
            var dto = new BranchPinCodeDto
            {
                BranchId = pinCode.BranchId,
                PinCode = vm.PinCode,
                IsActive = vm.IsActive
            };
            await _pinCodeService.Update(pinCode, dto);
            _notificationHelper.SetSuccessMsg("Pin Code updated successfully");
            return RedirectToAction(nameof(Report));
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
            var pinCode = await _pinCodeRepo.FindOrThrowAsync(id);
            await _pinCodeService.Delete(pinCode);
            _notificationHelper.SetSuccessMsg("Pin Code deleted successfully");
            return RedirectToAction(nameof(Report));
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return RedirectToAction(nameof(Report));
        }
    }
}
