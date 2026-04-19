using System.Transactions;
using Base.Dtos;
using Base.Entities;
using Base.Enum;
using Base.Manager.Interface;
using Base.Repo.Interfaces;
using Base.Services.Interfaces;
using Base.Validator.Interface;

namespace Base.Manager;

public class EmployeeManager : IEmployeeManager
{
    private readonly IEmployeeService _employeeService;
    private readonly IUserService _userService;
    private readonly IUow _uow;
    private readonly IEmployeeRepo _employeeRepo;
    private readonly IDriverRepo _driverRepo;
    private readonly IBranchRepo _branchRepo;
    private readonly IEmployeeValidator _employeeValidator;

    public EmployeeManager(IEmployeeService employeeService, IUserService userService,
        IUow uow, IEmployeeRepo employeeRepo, IDriverRepo driverRepo,
        IBranchRepo branchRepo, IEmployeeValidator employeeValidator)
    {
        _employeeService = employeeService;
        _userService = userService;
        _uow = uow;
        _employeeRepo = employeeRepo;
        _driverRepo = driverRepo;
        _branchRepo = branchRepo;
        _employeeValidator = employeeValidator;
    }

    public async Task Create(EmployeeAddDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        // Validate user account fields if creating login
        _employeeValidator.ValidateUserAccountFields(dto.CreateUserAccount, dto.Email, dto.Password, dto.ConfirmPassword);

        // Validate driver fields if employee type is Driver
        if (dto.EmployeeType == EmployeeType.Driver)
            _employeeValidator.ValidateDriverFields(dto.LicenseNumber, dto.LicenseExpiry);

        // 1. Create Employee
        var employee = await _employeeService.Create(dto);

        // 2. If creating user account
        if (dto.CreateUserAccount)
        {
            var branch = await _branchRepo.FindOrThrowAsync(dto.CurrentBranchId);
            var userDto = new UserAddDto
            {
                Name = dto.Name,
                Email = dto.Email!,
                Username = dto.Username!,
                Password = dto.Password!,
                ContactNo = dto.Phone,
                Address = dto.Address,
                UserLevel = UserLevel.User,
                IsActive = true
            };
            var user = await _userService.Create(userDto, branch);
            employee.UserId = user.Id;
            _uow.Update(employee);
            await _uow.CommitAsync();
        }

        // 3. If employee type is Driver, create Driver entity
        if (dto.EmployeeType == EmployeeType.Driver)
        {
            var driver = new Driver
            {
                LicenseNumber = dto.LicenseNumber!,
                LicenseExpiry = dto.LicenseExpiry!.Value,
                EmployeeId = employee.Id
            };
            await _uow.CreateAsync(driver);
            await _uow.CommitAsync();
        }

        tx.Complete();
    }

    public async Task Update(long id, EmployeeUpdateDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var employee = await _employeeRepo.FindOrThrowAsync(id);

        // Validate driver fields if changing to Driver type
        if (dto.EmployeeType == EmployeeType.Driver)
            _employeeValidator.ValidateDriverFields(dto.LicenseNumber, dto.LicenseExpiry);

        await _employeeService.Update(employee, dto);

        // Handle Driver entity
        var existingDriver = await _driverRepo.FindSingleAsync(d => d.EmployeeId == id);

        if (dto.EmployeeType == EmployeeType.Driver)
        {
            if (existingDriver != null)
            {
                existingDriver.LicenseNumber = dto.LicenseNumber!;
                existingDriver.LicenseExpiry = dto.LicenseExpiry!.Value;
                _uow.Update(existingDriver);
            }
            else
            {
                var driver = new Driver
                {
                    LicenseNumber = dto.LicenseNumber!,
                    LicenseExpiry = dto.LicenseExpiry!.Value,
                    EmployeeId = id
                };
                await _uow.CreateAsync(driver);
            }
            await _uow.CommitAsync();
        }
        else if (existingDriver != null)
        {
            // Employee type changed from Driver to something else, remove driver record
            _uow.Remove(existingDriver);
            await _uow.CommitAsync();
        }

        tx.Complete();
    }

    public async Task Delete(long id)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var employee = await _employeeRepo.FindOrThrowAsync(id);

        // Remove driver if exists
        var driver = await _driverRepo.FindSingleAsync(d => d.EmployeeId == id);
        if (driver != null)
            _uow.Remove(driver);

        // Soft-delete the user if linked
        if (employee.UserId.HasValue)
        {
            var userRepo = _uow.Repo<IUserRepo>();
            var user = await userRepo.FindByIdAsync(employee.UserId.Value);
            if (user != null)
            {
                user.IsActive = false;
                _uow.Update(user);
            }
        }

        await _employeeService.Delete(employee);

        tx.Complete();
    }
}
