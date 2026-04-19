using System.Transactions;
using Base.Constants;
using Base.Dtos;
using Base.Entities;
using Base.Enum;
using Base.Helpers;
using Base.Providers.Interfaces;
using Base.Repo.Interfaces;
using Base.Services.Interfaces;
using Base.Validator.Interface;
using Microsoft.EntityFrameworkCore;

namespace Base.Services;

public class UserService : IUserService
{
    private readonly IUow _uow;
    private readonly IUserRepo _userRepo;
    private readonly IBranchRepo _branchRepo;
    private readonly IUserValidator _userValidator;
    private readonly IUserRoleService _userRoleService;
    private readonly IRoleRepo _roleRepo;
    private readonly ICurrentUserProvider _currentUserProvider;

    public UserService(IUow uow, IUserRepo userRepo, IBranchRepo branchRepo,
        IUserValidator userValidator, IUserRoleService userRoleService,
        IRoleRepo roleRepo, ICurrentUserProvider currentUserProvider)
    {
        _uow = uow;
        _userRepo = userRepo;
        _branchRepo = branchRepo;
        _userValidator = userValidator;
        _userRoleService = userRoleService;
        _roleRepo = roleRepo;
        _currentUserProvider = currentUserProvider;
    }

    private long? GetCurrentUserIdSafe()
    {
        try { return _currentUserProvider.GetUserId(); }
        catch { return null; }
    }

    public async Task RegisterAdminUser()
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        if (await _userRepo.CheckIfExistAsync(x => x.Id == (long)IdConstants.MainUserId))
            throw new Exception("Admin user already registered");
        var branch = new Branch()
        {
            Id = (long)IdConstants.MainBranchId,
            Name = "Main Branch",
            Address = "Birtamode",
            Code = "001",
            ContactNo = "980000000",
            Status = StatusEnum.Active
        };
        await _uow.CreateAsync(branch);
        var user = new User()
        {
            Id = (long)IdConstants.MainUserId,
            Name = "Admin",
            UserName = "admin",
            Email = "qbmin@gmail.com",
            NormalizedEmail = "QBMIN@GMAIL.COM",
            NormalizedUserName = "ADMIN",
            ContactNo = "980000000",
            BranchId = (long)IdConstants.MainBranchId,
            PasswordHash = Crypter.Encrypt("Admin@123"),
            SecurityStamp = "",
            UserLevel = UserLevel.SuperAdmin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _uow.CreateAsync(user);
        await _uow.CommitAsync();
        scope.Complete();
    }

    public async Task<User> Create(UserAddDto dto, Branch branch)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        _userValidator.ValidatePassword(dto.Password);
        if (string.IsNullOrEmpty(dto.Username))
        {
            dto.Username = dto.Email;
        }
        var user = new User
        {
            Name = dto.Name,
            UserName = dto.Username,
            NormalizedUserName = dto.Username.ToUpper(),
            Email = dto.Email,
            NormalizedEmail = dto.Email.ToUpper(),
            ContactNo = dto.ContactNo,
            Address = dto.Address,
            Branch = branch,
            PasswordHash = Crypter.Encrypt(dto.Password),
            SecurityStamp = "",
            UserLevel = dto.UserLevel,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedById = GetCurrentUserIdSafe()
        };
        await _uow.CreateAsync(user);
        _userValidator.ValidateUserEmail(user, dto.Email);
        await _uow.CommitAsync();
        scope.Complete();
        return user;
    }

    public async Task Update(User user, UserUpdateDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        _userValidator.ValidateUserEmail(user, dto.Email);
        user.Name = dto.Name;
        user.Address = dto.Address;
        user.ContactNo = dto.ContactNo;
        user.Email = dto.Email;
        user.IsActive = dto.IsActive;
        _uow.Update(user);
        await _uow.CommitAsync();
        tx.Complete();
    }

    public async Task UpdatePassword(User user, string oldPass, string newPass)
    {
        if (!Crypter.IsMatching(oldPass, user.PasswordHash))
            throw new Exception("Failed to match old password");
        _userValidator.ValidatePassword(newPass);
        user.PasswordHash = Crypter.Encrypt(newPass);
        _uow.Update(user);
        await _uow.CommitAsync();
    }

    public async Task Delete(User user)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        _uow.Remove(user);
        await _uow.CommitAsync();
        tx.Complete();
    }
    
    public async Task ResetPassword(User user, string newPass)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        _userValidator.ValidatePassword(newPass);
        user.PasswordHash = Crypter.Encrypt(newPass);
        _uow.Update(user);
        await _uow.CommitAsync();
        tx.Complete();
    }

    public async Task<UserImportResultDto> ImportFromExcel(UserImportRequestDto request)
    {
        var result = new UserImportResultDto { TotalRows = request.Users.Count };
        var validatedUsers = new List<(UserImportDto dto, Branch branch, string password)>();
        var roleList = request.Users.SelectMany(u => u.Role.Split(",")).Distinct().ToList();
        var roles = await _roleRepo.GetQueryable().ToListAsync();

        // Step 1: Validate default password
        try
        {
            _userValidator.ValidatePassword(request.DefaultPassword);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add(new UserImportErrorDto
            {
                RowNumber = 0,
                ErrorMessage = $"Default password invalid: {ex.Message}"
            });
            return result;
        }

        // Step 2: Pre-validate all rows (no DB writes)
        foreach (var dto in request.Users)
        {
            try
            {
                // Required field validation
                if (string.IsNullOrWhiteSpace(dto.Name))
                    throw new Exception("Name is required");
                if (string.IsNullOrWhiteSpace(dto.Email))
                    throw new Exception("Email is required");
                if (string.IsNullOrWhiteSpace(dto.ContactNo))
                    throw new Exception("Contact No is required");
                if (string.IsNullOrWhiteSpace(dto.BranchCode))
                    throw new Exception("BranchCode is required");
                if (string.IsNullOrWhiteSpace(dto.UserLevel))
                    throw new Exception("UserLevel is required");
                if (string.IsNullOrWhiteSpace(dto.Role))
                    throw new Exception("Role is required");

                // Email format validation
                if (!dto.Email.Contains("@"))
                    throw new Exception("Invalid email format");

                // UserLevel validation
                if (!int.TryParse(dto.UserLevel, out int userLevelInt) || userLevelInt < 1 || userLevelInt > 4)
                    throw new Exception("UserLevel must be 1, 2, 3, or 4 (1=SuperAdmin, 2=Admin, 3=BranchAdmin, 4=User)");

                // Password selection and validation
                var password = string.IsNullOrWhiteSpace(dto.Password) ? request.DefaultPassword : dto.Password.Trim();
                _userValidator.ValidatePassword(password);

                // Check duplicate email in Excel
                if (validatedUsers.Any(v => v.dto.Email.Trim().ToLower() == dto.Email.Trim().ToLower()))
                    throw new Exception($"Duplicate email in Excel: {dto.Email}");

                // Check duplicate email in database
                if (_userRepo.CheckIfExist(u => u.Email.ToLower() == dto.Email.Trim().ToLower()))
                    throw new Exception($"Email already exists in database: {dto.Email}");

                // Validate branch exists
                var branch = _branchRepo.GetQueryable()
                    .FirstOrDefault(b => b.Code == dto.BranchCode.Trim());
                if (branch == null)
                    throw new Exception($"Branch with code '{dto.BranchCode}' does not exist");

                validatedUsers.Add((dto, branch, password));
            }
            catch (Exception ex)
            {
                result.Errors.Add(new UserImportErrorDto
                {
                    RowNumber = dto.RowNumber,
                    ErrorMessage = ex.Message
                });
            }
        }

        // Step 3: If any validation errors, return without saving
        if (result.Errors.Any())
        {
            result.Success = false;
            return result;
        }

        // Step 4: Bulk insert with transaction (all-or-nothing)
        var rlDtos = new List<UserRoleDto>();
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        try
        {
            foreach (var (dto, branch, password) in validatedUsers)
            {
                var userAddDto = new UserAddDto
                {
                    Name = dto.Name.Trim(),
                    Email = dto.Email.Trim(),
                    ContactNo = dto.ContactNo.Trim(),
                    Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim(),
                    Password = password,
                    UserLevel = (UserLevel)int.Parse(dto.UserLevel)
                };

                var user = await Create(userAddDto, branch);
                var rls = dto.Role.Split(",");
                rlDtos.Add(new UserRoleDto()
                {
                    Branch =  branch,
                    Roles = rls.Select(rl => roles.FirstOrDefault(x => x.Name.Trim().ToLower().Equals(rl.Trim().ToLower()))
                                            ?? throw new Exception($"Role with name '{rl.Trim()}' does not exist")).ToList(),
                    User = user
                });
            }

            await _uow.CommitAsync();
            await _userRoleService.AssignRole(rlDtos);
            tx.Complete();

            result.Success = true;
            result.SuccessCount = validatedUsers.Count;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add(new UserImportErrorDto
            {
                RowNumber = 0,
                ErrorMessage = $"Database error: {ex.Message}"
            });
        }

        return result;
    }
}