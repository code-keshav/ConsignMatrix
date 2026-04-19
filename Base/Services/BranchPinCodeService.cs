using System.Transactions;
using Base.Dtos;
using Base.Entities;
using Base.Repo.Interfaces;
using Base.Services.Interfaces;

namespace Base.Services;

public class BranchPinCodeService : IBranchPinCodeService
{
    private readonly IUow _uow;
    private readonly IBranchPinCodeRepo _pinCodeRepo;

    public BranchPinCodeService(IUow uow, IBranchPinCodeRepo pinCodeRepo)
    {
        _uow = uow;
        _pinCodeRepo = pinCodeRepo;
    }

    public async Task<BranchPinCode> Create(BranchPinCodeDto dto)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        if (_pinCodeRepo.CheckIfExist(p => p.BranchId == dto.BranchId && p.PinCode == dto.PinCode))
            throw new Exception($"Pin code '{dto.PinCode}' already exists for this branch");

        var pinCode = new BranchPinCode
        {
            BranchId = dto.BranchId,
            PinCode = dto.PinCode,
            IsActive = dto.IsActive
        };

        await _uow.CreateAsync(pinCode);
        await _uow.CommitAsync();
        scope.Complete();
        return pinCode;
    }

    public async Task Update(BranchPinCode pinCode, BranchPinCodeDto dto)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        if (_pinCodeRepo.CheckIfExist(p => p.BranchId == pinCode.BranchId && p.PinCode == dto.PinCode && p.Id != pinCode.Id))
            throw new Exception($"Pin code '{dto.PinCode}' already exists for this branch");

        pinCode.PinCode = dto.PinCode;
        pinCode.IsActive = dto.IsActive;
        _uow.Update(pinCode);
        await _uow.CommitAsync();
        scope.Complete();
    }

    public async Task Delete(BranchPinCode pinCode)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        _uow.Remove(pinCode);
        await _uow.CommitAsync();
        scope.Complete();
    }

    public async Task<List<Branch>> CheckServiceability(string pinCode)
    {
        return await _pinCodeRepo.GetBranchesServingPinCode(pinCode);
    }
}
