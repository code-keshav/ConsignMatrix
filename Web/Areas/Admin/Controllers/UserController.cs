using Acl.Helper.Interface;
using Base.Constants;
using Base.Dtos;
using Base.Entities;
using Base.Enum;
using Base.Providers.Interfaces;
using Base.Repo.Interfaces;
using Base.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Web.Areas.Admin.Requests;
using Web.Areas.Admin.ViewModels;
using Web.Extensions;
using Web.Helpers;

namespace Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("[area]/[controller]/[action]")]
public class UserController : Controller
{
    private readonly IUserService _userService;
    private readonly IUserRepo _userRepo;
    private readonly INotificationHelper _notificationHelper;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IRoleRepo _roleRepo;
    private readonly IUserRoleRepo _userRoleRepo;
    private readonly IBranchRepo _branchRepo;
    private readonly IPermissionChecker _permissionChecker;

    public UserController(IUserService userService, IUserRepo userRepo, INotificationHelper notificationHelper,
        ICurrentUserProvider currentUserProvider, IRoleRepo roleRepo, IUserRoleRepo userRoleRepo, IBranchRepo branchRepo, IPermissionChecker permissionChecker)
    {
        _userService = userService;
        _userRepo = userRepo;
        _notificationHelper = notificationHelper;
        _currentUserProvider = currentUserProvider;
        _roleRepo = roleRepo;
        _userRoleRepo = userRoleRepo;
        _branchRepo = branchRepo;
        _permissionChecker = permissionChecker;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> RegisterInitial()
    {
        try
        {
            await _userService.RegisterAdminUser();
            return this.SendSuccess("Admin user register successfully");
        }
        catch (Exception e)
        {
            return this.SendError(e.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Index(string searchTerm, long? branchId, long? roleId, int? userLevel, int page = 1, int pageSize = 20)
    {
        var currentUser = await _currentUserProvider.GetCurrentUser();
        var isMainBranchUser = currentUser.BranchId == (long)IdConstants.MainBranchId;
        var hasPermission = await _permissionChecker.HasPermissionAsync(currentUser, "/Admin/UserBranchTransfer/RequestTransfer");

        // Base query with branch inclusion
        var query = _userRepo.GetQueryable().Include(x => x.Branch).AsQueryable();

        // Branch filter: Main branch users see all, others see only their branch
        if (!isMainBranchUser)
        {
            query = query.Where(x => x.BranchId == currentUser.BranchId);
        }
        else if (branchId.HasValue)
        {
            query = query.Where(x => x.BranchId == branchId.Value);
        }

        // Search filter (name, email, contact)
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.Trim().ToLower();
            query = query.Where(x =>
                x.Name.ToLower().Contains(search) ||
                x.Email.ToLower().Contains(search) ||
                x.ContactNo.Contains(search));
        }

        // UserLevel filter
        if (userLevel.HasValue)
        {
            query = query.Where(x => (int)x.UserLevel == userLevel.Value);
        }

        // Role filter
        if (roleId.HasValue)
        {
            var usersWithRole = _userRoleRepo.GetQueryable()
                .Where(ur => ur.RoleId == roleId.Value)
                .Select(ur => ur.UserId)
                .Distinct();
            query = query.Where(u => usersWithRole.Contains(u.Id));
        }

        // Get total count for pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var users = await query
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Map to view models
        var userVms = users.Select(user => new UserVm
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            ContactNo = user.ContactNo,
            Address = user.Address,
            BranchName = user.Branch.Name,
            BranchCode = user.Branch.Code,
            UserLevelDisplay = user.UserLevel.ToString(),
            Roles = _userRoleRepo.GetQueryable().Where(a => a.UserId == user.Id).Select(a => a.Role).AsEnumerable(),
            IsAdmin = currentUser.UserLevel == UserLevel.Admin,
            IsSuperAdmin = currentUser.UserLevel == UserLevel.SuperAdmin,
            IsMainBranchUser = isMainBranchUser,
            HasPermission = hasPermission,
            IsActive = user.IsActive
        }).ToList();

        // Get data for filters
        var roles = await _roleRepo.GetRoles();
        var branches = isMainBranchUser
            ? await _branchRepo.GetQueryable().OrderBy(b => b.Name).ToListAsync()
            : new List<Branch>();

        var viewModel = new UserWithRolesVm
        {
            Users = userVms,
            Roles = roles,
            Branches = branches,
            SearchTerm = searchTerm,
            BranchId = branchId,
            RoleId = roleId,
            UserLevel = userLevel,
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
        var vm = new UserCreateVm();
        var userBranchId = await _currentUserProvider.GetUserBranchId();
        vm.BranchId = userBranchId;
        vm.Branches = await _branchRepo.GetQueryable()
            .Where(x => userBranchId == (long)IdConstants.MainBranchId || x.Id == userBranchId)
            .ToListAsync();
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Create(UserCreateVm createVm)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                throw new Exception($"Error during model validation. \n {string.Join(Environment.NewLine, errors)}");
            }

            var userBranchId = await _currentUserProvider.GetUserBranchId();
            createVm.Branches = await _branchRepo.GetQueryable()
                .Where(x => userBranchId == (long)IdConstants.MainBranchId || x.Id == userBranchId)
                .ToListAsync();
            if (!createVm.Password.Trim().ToLower().Equals(createVm.ConfirmPassword.Trim().ToLower()))
                throw new Exception("Password and confirm password does not match");
            var branch = await _branchRepo.FindOrThrowAsync(userBranchId == (long)IdConstants.MainBranchId ? createVm.BranchId : userBranchId);
            var dto = new UserAddDto
            {
                Name = createVm.Name,
                Email = createVm.Email,
                Password = createVm.Password,
                ContactNo = createVm.ContactNo,
                Address = createVm.Address,
                UserLevel = UserLevel.User,
                IsActive = createVm.IsActive,
            };
            var user = await _userService.Create(dto, branch);
            _notificationHelper.SetSuccessMsg("User created successfully");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return View(createVm);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Update(long id)
    {
        try
        {
            var user = await _userRepo.FindOrThrowAsync(id);
            var userEditVm = new UserEditVm
            {
                Name = user.Name,
                Email = user.Email,
                ContactNo = user.ContactNo,
                Address = user.Address,
                IsActive = user.IsActive
            };
            return View(userEditVm);
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Update(UserEditVm vm)
    {
        try
        {
            var user = await _userRepo.FindOrThrowAsync(vm.Id);
            var dto = new UserUpdateDto
            {
                Name = vm.Name,
                ContactNo = vm.ContactNo,
                Address = vm.Address,
                Email = vm.Email,
                IsActive = vm.IsActive,
            };
            await _userService.Update(user, dto);
            _notificationHelper.SetSuccessMsg("User updated successfully");
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
            var user = await _userRepo.FindOrThrowAsync(id);
            await _userService.Delete(user);
            _notificationHelper.SetSuccessMsg($"User `{user.Name}` deleted successfully");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword([FromBody] UserPasswordUpdateVm vm)
    {
        try
        {
            var user = await _userRepo.FindOrThrowAsync(vm.Id);
            await _userService.UpdatePassword(user, vm.OldPassword, vm.NewPassword);
            return this.SendSuccess("Password updated successfully");
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return this.SendError(e.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> ResetPassword(long id)
    {
        var user = await _userRepo.FindOrThrowAsync(id);
        var vm = new UserResetPasswordVm
        {
            Id = id,
            Name = user.Name
        };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(UserResetPasswordVm vm)
    {
        try
        {
            var user = await _userRepo.FindOrThrowAsync(vm.Id);
            if (!user.IsAdmin() || !user.IsSuperAdmin())
                throw new Exception("You are not allowed to reset this user password. Please contact your administrator");
            await _userService.ResetPassword(user, vm.NewPassword);
            _notificationHelper.SetSuccessMsg("User password reset successfully");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return this.SendError(e.Message);
        }
    }

    [HttpGet]
    public IActionResult Import()
    {
        return View(new UserImportVm());
    }

    [HttpPost]
    public async Task<IActionResult> Import(UserImportVm vm)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            if (vm.File == null || vm.File.Length == 0)
            {
                _notificationHelper.SetErrorMsg("Please select a file to upload");
                return View(vm);
            }

            var extension = Path.GetExtension(vm.File.FileName).ToLower();
            if (extension != ".xlsx")
            {
                _notificationHelper.SetErrorMsg("Only .xlsx files are supported");
                return View(vm);
            }

            if (vm.File.Length > 10 * 1024 * 1024)
            {
                _notificationHelper.SetErrorMsg("File size cannot exceed 10MB");
                return View(vm);
            }

            var users = await ParseExcelFile(vm.File);

            if (!users.Any())
            {
                _notificationHelper.SetErrorMsg("No valid data found in Excel file");
                return View(vm);
            }

            if (users.Any(x => string.IsNullOrEmpty(x.Password)) && string.IsNullOrEmpty(vm.DefaultPassword))
            {
                throw new Exception("Password not set for all users in sheet. In that case enter Default password in form");
            }

            var request = new UserImportRequestDto
            {
                Users = users,
                DefaultPassword = vm.DefaultPassword
            };
            var result = await _userService.ImportFromExcel(request);

            // Convert to ViewModel
            var resultVm = new UserImportResultVm
            {
                Success = result.Success,
                TotalRows = result.TotalRows,
                SuccessCount = result.SuccessCount,
                Errors = result.Errors.Select(e => new UserImportErrorVm
                {
                    RowNumber = e.RowNumber,
                    ErrorMessage = e.ErrorMessage
                }).ToList()
            };

            if (result.Success)
            {
                _notificationHelper.SetSuccessMsg($"Successfully imported {result.SuccessCount} users");
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

    private async Task<List<UserImportDto>> ParseExcelFile(IFormFile file)
    {
        var users = new List<UserImportDto>();

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
            for (int col = 1; col <= 7; col++)
            {
                if (worksheet.Cells[row, col].Value != null)
                {
                    isEmpty = false;
                    break;
                }
            }

            if (isEmpty) continue;

            var user = new UserImportDto
            {
                RowNumber = row,
                Name = worksheet.Cells[row, 1].Value?.ToString()?.Trim(),
                Email = worksheet.Cells[row, 2].Value?.ToString()?.Trim(),
                ContactNo = worksheet.Cells[row, 3].Value?.ToString()?.Trim(),
                Address = worksheet.Cells[row, 4].Value?.ToString()?.Trim(),
                BranchCode = worksheet.Cells[row, 5].Value?.ToString()?.Trim(),
                UserLevel = worksheet.Cells[row, 6].Value?.ToString()?.Trim(),
                Password = worksheet.Cells[row, 7].Value?.ToString()?.Trim(),
                Role = worksheet.Cells[row, 8].Value?.ToString()?.Trim()
            };

            users.Add(user);
        }

        users = users.Where(x => !x.Name.Equals(GetUserImportAcessLevelNote())).ToList();
        return users;
    }

    [HttpGet]
    public IActionResult DownloadUserSample()
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Users");

        // Add headers
        worksheet.Cells[1, 1].Value = "Name";
        worksheet.Cells[1, 2].Value = "Email";
        worksheet.Cells[1, 3].Value = "ContactNo";
        worksheet.Cells[1, 4].Value = "Address";
        worksheet.Cells[1, 5].Value = "BranchCode";
        worksheet.Cells[1, 6].Value = "UserLevel";
        worksheet.Cells[1, 7].Value = "Password";
        worksheet.Cells[1, 8].Value = "Role";

        // Style headers
        using (var range = worksheet.Cells[1, 1, 1, 7])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
        }

        // Add sample data
        worksheet.Cells[2, 1].Value = "John Doe";
        worksheet.Cells[2, 2].Value = "john.doe@example.com";
        worksheet.Cells[2, 3].Value = "9841234567";
        worksheet.Cells[2, 4].Value = "Kathmandu, Nepal";
        worksheet.Cells[2, 5].Value = "MB001";
        worksheet.Cells[2, 6].Value = "4";
        worksheet.Cells[2, 7].Value = "User@123";
        worksheet.Cells[2, 8].Value = "Admin";

        worksheet.Cells[3, 1].Value = "Jane Smith";
        worksheet.Cells[3, 2].Value = "jane.smith@example.com";
        worksheet.Cells[3, 3].Value = "9849876543";
        worksheet.Cells[3, 4].Value = "Pokhara, Nepal";
        worksheet.Cells[3, 5].Value = "SB001";
        worksheet.Cells[3, 6].Value = "4";
        worksheet.Cells[3, 7].Value = ""; // Empty - will use default password
        worksheet.Cells[3, 8].Value = "Branch Manager"; 

        // Add comment/note row for UserLevel values
        worksheet.Cells[5, 1].Value = GetUserImportAcessLevelNote();
        worksheet.Cells[5, 1, 5, 7].Merge = true;
        worksheet.Cells[5, 1].Style.Font.Italic = true;
        worksheet.Cells[5, 1].Style.Font.Color.SetColor(System.Drawing.Color.Gray);

        // Auto-fit columns
        worksheet.Cells.AutoFitColumns();

        var stream = new MemoryStream(package.GetAsByteArray());
        var fileName = "UserImportTemplate.xlsx";

        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    private static string GetUserImportAcessLevelNote()
    {
        return "Note: UserLevel values - 1=SuperAdmin, 2=Admin, 3=BranchAdmin, 4=User";
    }
}