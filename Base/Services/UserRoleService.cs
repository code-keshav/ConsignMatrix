using System.Transactions;
using Base.Dtos;
using Base.Entities;
using Base.Repo.Interfaces;
using Base.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Base.Services;

public class UserRoleService : IUserRoleService
{
    private readonly IUserRoleRepo _userRoleRepo;
    private readonly IUow _uow;

    public UserRoleService(IUserRoleRepo userRoleRepo, IUow uow)
    {
        _userRoleRepo = userRoleRepo;
        _uow = uow;
    }

    public async Task AssignRole(UserRoleDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var existingUserRole = await _userRoleRepo.GetQueryable().Where(a => a.UserId == dto.User.Id && a.BranchId == dto.Branch.Id).ToListAsync();
        if (existingUserRole.Count > 0)
        {
            _uow.RemoveRange(existingUserRole);
            await _uow.CommitAsync();
        }

        foreach (var role in dto.Roles)
        {
            var userRole = new UserRole(dto.User, role, dto.Branch);
            await _uow.CreateAsync(userRole);
            await _uow.CommitAsync();
        }

        tx.Complete();
    }

    public async Task UnassignRole(UserRole userRole)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        _uow.Remove(userRole);
        await _uow.CommitAsync();
        tx.Complete();
    }

    public async Task AssignRole(List<UserRoleDto> rlDtos)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var existingUserRoles = await _userRoleRepo.GetQueryable().ToListAsync();
        var userRoles = new List<UserRole>();
        foreach (var item in rlDtos)
        {
            foreach (var itemRole in item.Roles)
            {
                if(existingUserRoles.Any(x => x.UserId == item.User.Id && x.RoleId == itemRole.Id)) continue;
                userRoles.Add(new UserRole(item.User, itemRole, item.Branch));
            }
        }
        await _uow.CreateRangeAsync(userRoles);
        await _uow.CommitAsync();
        tx.Complete();
    }

    private async Task UpdateUserRole(UserRole userRole, Role role)
    {
        userRole.UpdateRole(role);
        _uow.Update(userRole);
        await _uow.CommitAsync();
    }

    // private async Task CreateUserRole(UserRoleDto dto)
    // {
    //     var userRole = new UserRole(dto.User, dto.Roles, dto.Branch);
    //     await _uow.CreateAsync(userRole);
    //     await _uow.CommitAsync();
    // }
}