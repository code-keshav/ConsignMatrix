using System.Transactions;
using Base.Dtos;
using Base.Entities;
using Base.Enum;
using Base.Repo.Interfaces;
using Base.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Base.Services;

public class BranchService : IBranchService
{
    private readonly IUow _uow;
    private readonly IBranchRepo _branchRepo;

    public BranchService(IUow uow, IBranchRepo branchRepo)
    {
        _uow = uow;
        _branchRepo = branchRepo;
    }

    public async Task<Branch> Create(BranchDto dto)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var branch = new Branch
        {
            Name = dto.Name,
            Code = dto.Code,
            BranchType = dto.BranchType,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            ContactNo = dto.ContactNo,
            Email = dto.Email,
            StorageCapacity = dto.StorageCapacity,
            OperatingHours = dto.OperatingHours,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude
        };
        ValidateBranchCode(branch, dto.Code);
        if (dto.Email != null) ValidateBranchEmail(branch, dto.Email);
        await _uow.CreateAsync(branch);
        await _uow.CommitAsync();
        scope.Complete();
        return branch;
    }

    public async Task Update(Branch branch, BranchDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        ValidateBranchCode(branch, dto.Code);
        if (dto.Email != null) ValidateBranchEmail(branch, dto.Email);
        branch.Name = dto.Name;
        branch.Code = dto.Code;
        branch.BranchType = dto.BranchType;
        branch.Address = dto.Address;
        branch.City = dto.City;
        branch.State = dto.State;
        branch.ContactNo = dto.ContactNo;
        branch.Email = dto.Email;
        branch.StorageCapacity = dto.StorageCapacity;
        branch.OperatingHours = dto.OperatingHours;
        branch.Latitude = dto.Latitude;
        branch.Longitude = dto.Longitude;
        _uow.Update(branch);
        await _uow.CommitAsync();
        tx.Complete();
    }

    public async Task Activate(Branch branch)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        if (branch.Status == StatusEnum.Active)
            throw new Exception("Branch is already active");
        branch.Status = StatusEnum.Active;
        _uow.Update(branch);
        await _uow.CommitAsync();
        tx.Complete();
    }

    public async Task Deactivate(Branch branch)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        if (branch.Status == StatusEnum.Inactive)
            throw new Exception("Branch is already inactive");
        branch.Status = StatusEnum.Inactive;
        _uow.Update(branch);
        await _uow.CommitAsync();
        tx.Complete();
    }

    public async Task<BranchImportResultDto> ImportFromExcel(List<BranchImportDto> branches)
    {
        var result = new BranchImportResultDto { TotalRows = branches.Count };
        var validBranches = new List<Branch>();

        // Step 1: Pre-validate all rows (no DB operations)
        foreach (var dto in branches)
        {
            try
            {
                // Required field validation
                if (string.IsNullOrWhiteSpace(dto.Name))
                    throw new Exception("Name is required");
                if (string.IsNullOrWhiteSpace(dto.Code))
                    throw new Exception("Code is required");
                if (string.IsNullOrWhiteSpace(dto.Address))
                    throw new Exception("Address is required");
                if (string.IsNullOrWhiteSpace(dto.ContactNo))
                    throw new Exception("Contact No is required");

                // Check duplicate code in Excel
                if (validBranches.Any(b => b.Code == dto.Code))
                    throw new Exception($"Duplicate code in Excel: {dto.Code}");

                // Check duplicate code in database
                if (_branchRepo.CheckIfExist(b => b.Code == dto.Code))
                    throw new Exception($"Branch code already exists in database: {dto.Code}");

                // Check duplicate email (if provided)
                if (!string.IsNullOrWhiteSpace(dto.Email))
                {
                    if (validBranches.Any(b => b.Email?.Trim() == dto.Email.Trim()))
                        throw new Exception($"Duplicate email in Excel: {dto.Email}");
                    if (_branchRepo.CheckIfExist(b => b.Email.Trim() == dto.Email.Trim()))
                        throw new Exception($"Branch email already exists in database: {dto.Email}");
                }

                var branchType = BranchType.ServiceCenter;
                if (!string.IsNullOrWhiteSpace(dto.BranchType))
                {
                    if (!System.Enum.TryParse<BranchType>(dto.BranchType.Trim(), true, out branchType))
                        throw new Exception($"Invalid BranchType: {dto.BranchType}. Use 'ServiceCenter' or 'Hub'");
                }

                var branch = new Branch
                {
                    Name = dto.Name.Trim(),
                    Code = dto.Code.Trim(),
                    BranchType = branchType,
                    Address = dto.Address.Trim(),
                    City = string.IsNullOrWhiteSpace(dto.City) ? null : dto.City.Trim(),
                    State = string.IsNullOrWhiteSpace(dto.State) ? null : dto.State.Trim(),
                    ContactNo = dto.ContactNo.Trim(),
                    Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
                    Status = StatusEnum.Active
                };

                validBranches.Add(branch);
            }
            catch (Exception ex)
            {
                result.Errors.Add(new BranchImportErrorDto
                {
                    RowNumber = dto.RowNumber,
                    ErrorMessage = ex.Message
                });
            }
        }

        // Step 2: If any validation errors, return without saving
        if (result.Errors.Any())
        {
            result.Success = false;
            return result;
        }

        // Step 3: Bulk insert with transaction (all-or-nothing)
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        try
        {
            foreach (var branch in validBranches)
            {
                await _uow.CreateAsync(branch);
            }
            await _uow.CommitAsync();
            tx.Complete();

            result.Success = true;
            result.SuccessCount = validBranches.Count;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add(new BranchImportErrorDto
            {
                RowNumber = 0,
                ErrorMessage = $"Database error: {ex.Message}"
            });
        }

        return result;
    }

    public async Task Delete(Branch branch)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        // Check if branch has ANY users
        var userCount = await _uow.Repo<IUserRepo>()
            .GetQueryable()
            .CountAsync(u => u.BranchId == branch.Id);

        if (userCount > 0)
        {
            throw new Exception($"Cannot delete branch '{branch.Name}'. It has {userCount} user(s) associated with it. Please transfer or delete the users first.");
        }

        _uow.Remove(branch);
        await _uow.CommitAsync();
        tx.Complete();
    }

    private void ValidateBranchCode(Branch branch, string code)
    {
        if (_branchRepo.CheckIfExist(a => a.Code == code && a.Id != branch.Id))
            throw new Exception($"Branch with code `{code}` already exists");
    }

    private void ValidateBranchEmail(Branch branch, string email)
    {
        if (_branchRepo.CheckIfExist(a => a.Id != branch.Id && a.Email.Trim() == email.Trim()))
            throw new Exception($"Branch with email `{email}` already exists");
    }
}