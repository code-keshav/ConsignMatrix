using System.Transactions;
using Base.Dtos;
using Base.Entities;
using Base.Repo.Interfaces;
using Base.Services.Interfaces;
using Base.Validator.Interface;

namespace Base.Services;

public class RoleService : IRoleService
{
    private readonly IUow _uow;
    private readonly IUserRoleRepo _userRoleRepo;
    private readonly IRoleValidator _roleValidator;

    public RoleService(IUow uow, IUserRoleRepo userRoleRepo, IRoleValidator roleValidator)
    {
        _uow = uow;
        _userRoleRepo = userRoleRepo;
        _roleValidator = roleValidator;
    }

    public async Task Create(RoleDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var role = new Role(dto.Name, dto.Priority, dto.Branch, dto.IsGlobal);
        _roleValidator.ValidateRoleName(role, dto.Name);
        await _uow.CreateAsync(role);
        await _uow.CommitAsync();
        tx.Complete();
    }

    public async Task Update(Role role, RoleEditDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        _roleValidator.ValidateRoleName(role, dto.Name);
        role.Name = dto.Name;
        role.Priority = dto.Priority;
        _uow.Update(role);
        await _uow.CommitAsync();
        tx.Complete();
    }

    public async Task Delete(Role role)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        _roleValidator.ValidateRoleUse(role);
        _uow.Remove(role);
        await _uow.CommitAsync();
        tx.Complete();
    }

    public async Task MarkAsGlobal(Role role, Branch branch)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        role.ToggleGlobalRole(true, branch);
        _uow.Update(role);
        await _uow.CommitAsync();
        tx.Complete();
    }

    public async Task UnmarkAsGlobal(Role role, Branch branch)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        _roleValidator.ValidateRoleUse(role);
        role.ToggleGlobalRole(false, branch);
        _uow.Update(role);
        await _uow.CommitAsync();
        tx.Complete();
    }
}