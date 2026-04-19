using System.Transactions;
using Acl.Dtos;
using Acl.Entities;
using Acl.Repo.Interfaces;
using Acl.Services.Interfaces;
using Base.Constants;
using Base.Repo.Interfaces;

namespace Acl.Services;

public class RolePermissionService : IRolePermissionService
{
    private readonly IUow _uow;
    private readonly IRolePermissionRepo _rolePermissionRepo;

    public RolePermissionService(IUow uow, IRolePermissionRepo rolePermissionRepo)
    {
        _uow = uow;
        _rolePermissionRepo = rolePermissionRepo;
    }

    public async Task UpdatePermission(List<RolePermissionDto> permissionDtos)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        foreach (var permissionDto in permissionDtos)
        {
            await DeleteRemovedPermissions(permissionDto);
            await AddPermissions(permissionDto);
        }

        await _uow.CommitAsync();
        tx.Complete();
    }

    private async Task AddPermissions(RolePermissionDto permissionDto)
    {
        var existingPermissions =
            await _rolePermissionRepo.GetPermissions(permissionDto.Role.Id, permissionDto.Branch.Id);
        var addablePermissions = new List<string>();
        foreach (var permissionToAdd in permissionDto.Permissions)
        {
            if (existingPermissions.Contains(permissionToAdd)) continue;
            addablePermissions.Add(permissionToAdd);
        }

        var permissions = addablePermissions.Select(x =>
            new RolePermission(x, permissionDto.Role, permissionDto.Branch)).ToList();
        await _uow.CreateRangeAsync(permissions);
        await _uow.CommitAsync();
    }


    private async Task DeleteRemovedPermissions(RolePermissionDto permissionDto)
    {
        var loweredPermissions = permissionDto.Permissions.Select(x => x.ToLower());
        var permissionsToRemove = await _rolePermissionRepo.FindByAsync(x =>
            (permissionDto.Branch.Id == (long)IdConstants.MainBranchId || x.BranchId == permissionDto.Branch.Id)
            && x.RoleId == permissionDto.Role.Id
            && !loweredPermissions.Contains(x.Permission.ToLower()));
        _rolePermissionRepo.RemoveRange(permissionsToRemove);
        await _uow.CommitAsync();
    }
}