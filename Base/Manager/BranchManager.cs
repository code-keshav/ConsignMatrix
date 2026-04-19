using System.Transactions;
using Base.Dtos;
using Base.Entities;
using Base.Enum;
using Base.Manager.Interface;
using Base.Repo.Interfaces;
using Base.Services.Interfaces;

namespace Base.Manager;

public class BranchManager : IBranchManager
{
    private readonly IBranchService _branchService;
    private readonly IUow _uow;

    public BranchManager(IBranchService branchService, IUow uow)
    {
        _branchService = branchService;
        _uow = uow;
    }

    public async Task Create(BranchDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await _branchService.Create(dto);
        await _uow.CommitAsync();
        tx.Complete();
    }
}