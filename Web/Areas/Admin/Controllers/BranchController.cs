using Base.Dtos;
using Base.Enum;
using Base.Manager.Interface;
using Base.Repo.Interfaces;
using Base.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Web.Areas.Admin.Responses;
using Web.Areas.Admin.ViewModels;
using Web.Extensions;
using Web.Helpers;

namespace Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("[area]/[controller]/[action]")]
public class BranchController : Controller
{
    private readonly IBranchService _branchService;
    private readonly IBranchRepo _branchRepo;
    private readonly IBranchManager _branchManager;
    private readonly INotificationHelper _notificationHelper;
    private readonly IUserRepo _userRepo;
    private readonly IBranchPinCodeRepo _branchPinCodeRepo;
    private readonly IBranchPinCodeService _branchPinCodeService;

    public BranchController(IBranchService branchService, IBranchRepo branchRepo, IBranchManager branchManager,
        INotificationHelper notificationHelper, IUserRepo userRepo,
        IBranchPinCodeRepo branchPinCodeRepo, IBranchPinCodeService branchPinCodeService)
    {
        _branchService = branchService;
        _branchRepo = branchRepo;
        _branchManager = branchManager;
        _notificationHelper = notificationHelper;
        _userRepo = userRepo;
        _branchPinCodeRepo = branchPinCodeRepo;
        _branchPinCodeService = branchPinCodeService;
    }

    [HttpGet]
    public async Task<IActionResult> Report()
    {
        try
        {
            var list = await (from branch in _branchRepo.GetQueryable()
                             join user in _userRepo.GetQueryable() on branch.Id equals user.BranchId into userGroup
                             select new BranchReportResponse
                             {
                                 Id = branch.Id,
                                 Name = branch.Name,
                                 Code = branch.Code,
                                 BranchType = branch.BranchType,
                                 Address = branch.Address,
                                 City = branch.City,
                                 ContactNo = branch.ContactNo,
                                 Email = branch.Email,
                                 Status = branch.Status,
                                 UserCount = userGroup.Count(),
                                 PinCodeCount = branch.PinCodes.Count(p => p.IsActive)
                             }).ToListAsync();
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
        return View();
    }


    [HttpPost]
    public async Task<IActionResult> Create(BranchVm vm)
    {
        try
        {
            var branchDto = new BranchDto
            {
                Name = vm.Name,
                Code = vm.Code,
                BranchType = vm.BranchType,
                Address = vm.Address,
                City = vm.City,
                State = vm.State,
                ContactNo = vm.ContactNo,
                Email = vm.Email,
                StorageCapacity = vm.StorageCapacity,
                OperatingHours = vm.OperatingHours,
                Latitude = vm.Latitude,
                Longitude = vm.Longitude,
            };
            await _branchManager.Create(branchDto);
            _notificationHelper.SetSuccessMsg("Branch Created Successfully");
            return RedirectToAction("Report");
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
            var branch = await _branchRepo.FindOrThrowAsync(id);
            var branchVm = new BranchEditVm
            {
                Id = branch.Id,
                Name = branch.Name,
                Code = branch.Code,
                BranchType = branch.BranchType,
                Address = branch.Address,
                City = branch.City,
                State = branch.State,
                ContactNo = branch.ContactNo,
                Email = branch.Email,
                StorageCapacity = branch.StorageCapacity,
                OperatingHours = branch.OperatingHours,
                Latitude = branch.Latitude,
                Longitude = branch.Longitude
            };
            return View(branchVm);
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return RedirectToAction(nameof(Report));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Update(BranchEditVm vm)
    {
        try
        {
            var branch = await _branchRepo.FindOrThrowAsync(vm.Id);
            var dto = new BranchDto
            {
                Name = vm.Name,
                Code = vm.Code,
                BranchType = vm.BranchType,
                Address = vm.Address,
                City = vm.City,
                State = vm.State,
                ContactNo = vm.ContactNo,
                Email = vm.Email,
                StorageCapacity = vm.StorageCapacity,
                OperatingHours = vm.OperatingHours,
                Latitude = vm.Latitude,
                Longitude = vm.Longitude
            };
            await _branchService.Update(branch, dto);
            _notificationHelper.SetSuccessMsg("Branch Updated Successfully");
            return RedirectToAction(nameof(Report));
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return RedirectToAction(nameof(Update));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Activate(long id)
    {
        try
        {
            var branch = await _branchRepo.FindOrThrowAsync(id);
            await _branchService.Activate(branch);
            return this.SendSuccess("", branch.ToMiniInfo);
        }
        catch (Exception e)
        {
            return this.SendError(e.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Deactivate(long id)
    {
        try
        {
            var branch = await _branchRepo.FindOrThrowAsync(id);
            await _branchService.Deactivate(branch);
            return this.SendSuccess("", branch.ToMiniInfo);
        }
        catch (Exception e)
        {
            return this.SendError(e.Message);
        }
    }

    [HttpGet]
    public IActionResult Import()
    {
        return View(new BranchImportVm());
    }

    [HttpPost]
    public async Task<IActionResult> Import(BranchImportVm vm)
    {
        try
        {
            if (vm.File == null || vm.File.Length == 0)
            {
                _notificationHelper.SetErrorMsg("Please select a file to upload");
                return View(vm);
            }

            // Validate file extension
            var extension = Path.GetExtension(vm.File.FileName).ToLower();
            if (extension != ".xlsx")
            {
                _notificationHelper.SetErrorMsg("Only .xlsx files are supported");
                return View(vm);
            }

            // Validate file size (max 10MB)
            if (vm.File.Length > 10 * 1024 * 1024)
            {
                _notificationHelper.SetErrorMsg("File size cannot exceed 10MB");
                return View(vm);
            }

            // Parse Excel file
            var branches = await ParseExcelFile(vm.File);

            if (!branches.Any())
            {
                _notificationHelper.SetErrorMsg("No valid data found in Excel file");
                return View(vm);
            }

            // Call service to import
            var result = await _branchService.ImportFromExcel(branches);

            // Convert to ViewModel
            var resultVm = new BranchImportResultVm
            {
                Success = result.Success,
                TotalRows = result.TotalRows,
                SuccessCount = result.SuccessCount,
                Errors = result.Errors.Select(e => new BranchImportErrorVm
                {
                    RowNumber = e.RowNumber,
                    ErrorMessage = e.ErrorMessage
                }).ToList()
            };

            if (result.Success)
            {
                _notificationHelper.SetSuccessMsg($"Successfully imported {result.SuccessCount} branches");
            }
            else
            {
                _notificationHelper.SetErrorMsg($"Import failed with {result.Errors.Count} errors");
            }

            return View("ImportResult", resultVm);
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return View(vm);
        }
    }

    private async Task<List<BranchImportDto>> ParseExcelFile(IFormFile file)
    {
        var branches = new List<BranchImportDto>();

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);

        using var package = new ExcelPackage(stream);

        if (package.Workbook.Worksheets.Count == 0)
            throw new Exception("Excel file has no worksheets");

        var worksheet = package.Workbook.Worksheets[0]; // First sheet
        var rowCount = worksheet.Dimension?.Rows ?? 0;

        if (rowCount < 2)
            throw new Exception("Excel file must have at least one data row besides header");

        // Start from row 2 (skip header)
        for (int row = 2; row <= rowCount; row++)
        {
            // Skip completely empty rows
            var isEmpty = true;
            for (int col = 1; col <= 8; col++)
            {
                if (worksheet.Cells[row, col].Value != null)
                {
                    isEmpty = false;
                    break;
                }
            }

            if (isEmpty) continue;

            var branch = new BranchImportDto
            {
                RowNumber = row,
                Name = worksheet.Cells[row, 1].Value?.ToString()?.Trim(),
                Code = worksheet.Cells[row, 2].Value?.ToString()?.Trim(),
                BranchType = worksheet.Cells[row, 3].Value?.ToString()?.Trim(),
                Address = worksheet.Cells[row, 4].Value?.ToString()?.Trim(),
                City = worksheet.Cells[row, 5].Value?.ToString()?.Trim(),
                State = worksheet.Cells[row, 6].Value?.ToString()?.Trim(),
                ContactNo = worksheet.Cells[row, 7].Value?.ToString()?.Trim(),
                Email = worksheet.Cells[row, 8].Value?.ToString()?.Trim()
            };

            branches.Add(branch);
        }

        return branches;
    }

    [HttpGet]
    public IActionResult DownloadSample()
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Branches");

        // Add headers
        worksheet.Cells[1, 1].Value = "Name";
        worksheet.Cells[1, 2].Value = "Code";
        worksheet.Cells[1, 3].Value = "BranchType";
        worksheet.Cells[1, 4].Value = "Address";
        worksheet.Cells[1, 5].Value = "City";
        worksheet.Cells[1, 6].Value = "State";
        worksheet.Cells[1, 7].Value = "ContactNo";
        worksheet.Cells[1, 8].Value = "Email";

        // Style headers
        using (var range = worksheet.Cells[1, 1, 1, 8])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
        }

        // Add sample data
        worksheet.Cells[2, 1].Value = "Main Branch";
        worksheet.Cells[2, 2].Value = "MB001";
        worksheet.Cells[2, 3].Value = "ServiceCenter";
        worksheet.Cells[2, 4].Value = "Kathmandu, Nepal";
        worksheet.Cells[2, 5].Value = "Kathmandu";
        worksheet.Cells[2, 6].Value = "Bagmati";
        worksheet.Cells[2, 7].Value = "01-4567890";
        worksheet.Cells[2, 8].Value = "main@example.com";

        worksheet.Cells[3, 1].Value = "Hub Branch";
        worksheet.Cells[3, 2].Value = "HB001";
        worksheet.Cells[3, 3].Value = "Hub";
        worksheet.Cells[3, 4].Value = "Pokhara, Nepal";
        worksheet.Cells[3, 5].Value = "Pokhara";
        worksheet.Cells[3, 6].Value = "Gandaki";
        worksheet.Cells[3, 7].Value = "061-456789";
        worksheet.Cells[3, 8].Value = "hub@example.com";

        // Auto-fit columns
        worksheet.Cells.AutoFitColumns();

        var stream = new MemoryStream(package.GetAsByteArray());
        var fileName = "BranchImportTemplate.xlsx";

        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            var branch = await _branchRepo.FindOrThrowAsync(id);
            await _branchService.Delete(branch);
            _notificationHelper.SetSuccessMsg($"Branch '{branch.Name}' deleted successfully");
            return RedirectToAction(nameof(Report));
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return RedirectToAction(nameof(Report));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetOptions()
    {
        try
        {
            var list = await _branchRepo.GetQueryable().Select(x => x.ToMiniInfo).ToListAsync();
            return this.SendSuccess("", list);
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return this.SendError(e.Message);
        }
    }

    // AJAX endpoints for inline pin code management on Update view

    [HttpGet]
    public async Task<IActionResult> GetPinCodes(long branchId)
    {
        try
        {
            var pinCodes = await _branchPinCodeRepo.GetQueryable()
                .Where(p => p.BranchId == branchId)
                .Select(p => new { p.Id, p.PinCode, p.IsActive })
                .ToListAsync();
            return this.SendSuccess("", pinCodes);
        }
        catch (Exception e)
        {
            return this.SendError(e.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddPinCode([FromBody] BranchPinCodeDto dto)
    {
        try
        {
            var pinCode = await _branchPinCodeService.Create(dto);
            return this.SendSuccess("Pin code added", new { pinCode.Id, pinCode.PinCode, pinCode.IsActive });
        }
        catch (Exception e)
        {
            return this.SendError(e.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> RemovePinCode([FromBody] long id)
    {
        try
        {
            var pinCode = await _branchPinCodeRepo.FindOrThrowAsync(id);
            await _branchPinCodeService.Delete(pinCode);
            return this.SendSuccess("Pin code removed");
        }
        catch (Exception e)
        {
            return this.SendError(e.Message);
        }
    }
}
