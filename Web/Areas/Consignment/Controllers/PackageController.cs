using Base.Dtos.Consignment;
using Base.Repo.Consignment.Interfaces;
using Base.Services.Consignment.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Web.Areas.Consignment.ViewModels.Package;
using Web.Helpers;

namespace Web.Areas.Consignment.Controllers;

[Area("Consignment")]
public class PackageController : Controller
{
    private readonly INotificationHelper _notificationHelper;
    private readonly IPackageService _packageService;
    private readonly IPackageRepo _packageRepo;
    private readonly IConsignmentRepo _consignmentRepo;

    public PackageController(
        INotificationHelper notificationHelper,
        IPackageService packageService,
        IPackageRepo packageRepo,
        IConsignmentRepo consignmentRepo)
    {
        _notificationHelper = notificationHelper;
        _packageService = packageService;
        _packageRepo = packageRepo;
        _consignmentRepo = consignmentRepo;
    }

    [HttpGet]
    public async Task<IActionResult> AddPackage(long id)
    {
        try
        {
            var consignment = await _consignmentRepo.FindOrThrowAsync(id);
            var vm = new PackageAddVm
            {
                ConsignmentId = id,
                TrackingNumber = consignment.TrackingNumber,
                CanBeStacked = true,
            };
            return View(vm);
        }
        catch (Exception ex)
        {
            _notificationHelper.SetErrorMsg(ex.Message);
            return RedirectToAction("Detail", "Consignment", new { id });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddPackage(PackageAddVm vm)
    {
        try
        {
            var dto = new PackageDto
            {
                Weight = vm.Weight,
                Length = vm.Length,
                Width = vm.Width,
                Height = vm.Height,
                PackageType = vm.PackageType,
                ContentDescription = vm.ContentDescription,
                IsFragile = vm.IsFragile,
                IsHazardous = vm.IsHazardous,
                CanBeStacked = vm.CanBeStacked,
                RequiresColdChain = vm.RequiresColdChain,
            };
            await _packageService.Create(vm.ConsignmentId, dto);
            return RedirectToAction("Detail", "Consignment", new { id = vm.ConsignmentId });
        }
        catch (Exception ex)
        {
            _notificationHelper.SetErrorMsg(ex.Message);
            return View(vm);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Update(long id, long consignmentId)
    {
        try
        {
            var package = await _packageRepo.FindOrThrowAsync(id);
            var vm = new PackageVm
            {
                Id = package.Id,
                ConsignmentId = package.ConsignmentId,
                PackageNumber = package.PackageNumber,
                Barcode = package.Barcode,
                Weight = package.Weight,
                Length = package.Length,
                Width = package.Width,
                Height = package.Height,
                Volume = package.Volume,
                VolumetricWeight = package.VolumetricWeight,
                PackageType = package.PackageType,
                ContentDescription = package.ContentDescription,
                IsFragile = package.IsFragile,
                IsHazardous = package.IsHazardous,
                CanBeStacked = package.CanBeStacked,
                RequiresColdChain = package.RequiresColdChain,
            };
            return View(vm);
        }
        catch (Exception ex)
        {
            _notificationHelper.SetErrorMsg(ex.Message);
            return RedirectToAction("Detail", "Consignment", new { id = consignmentId });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Update(PackageVm vm)
    {
        try
        {
            var package = await _packageRepo.FindOrThrowAsync(vm.Id);
            var dto = new PackageDto
            {
                Weight = vm.Weight,
                Length = vm.Length,
                Width = vm.Width,
                Height = vm.Height,
                PackageType = vm.PackageType,
                ContentDescription = vm.ContentDescription,
                IsFragile = vm.IsFragile,
                IsHazardous = vm.IsHazardous,
                CanBeStacked = vm.CanBeStacked,
                RequiresColdChain = vm.RequiresColdChain,
            };
            await _packageService.Update(package, dto);
            return RedirectToAction("Detail", "Consignment", new { id = vm.ConsignmentId });
        }
        catch (Exception ex)
        {
            _notificationHelper.SetErrorMsg(ex.Message);
            return RedirectToAction("Update", new { id = vm.Id, consignmentId = vm.ConsignmentId });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(long id, long consignmentId)
    {
        try
        {
            var package = await _packageRepo.FindOrThrowAsync(id);
            await _packageService.Delete(package);
            return RedirectToAction("Detail", "Consignment", new { id = consignmentId });
        }
        catch (Exception ex)
        {
            _notificationHelper.SetErrorMsg(ex.Message);
            return RedirectToAction("Detail", "Consignment", new { id = consignmentId });
        }
    }
}
