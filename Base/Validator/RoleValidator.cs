using Base.Entities;
using Base.Providers.Interfaces;
using Base.Repo.Interfaces;
using Base.Validator.Interface;

namespace Base.Validator;

public class RoleValidator : IRoleValidator
{
    private readonly IUserRoleRepo _userRoleRepo;
    private readonly IRoleRepo _roleRepo;
    private readonly ICurrentUserProvider _userProvider;

    public RoleValidator(IUserRoleRepo userRoleRepo, IRoleRepo roleRepo, ICurrentUserProvider userProvider)
    {
        _userRoleRepo = userRoleRepo;
        _roleRepo = roleRepo;
        _userProvider = userProvider;
    }

    public void ValidateRoleUse(Role role)
    {
        if (_userRoleRepo.CheckIfExist(a => a.RoleId == role.Id))
            throw new Exception("Role is already assigned to a user");
    }

    public void ValidateRoleName(Role role, string name)
    {
        if (_roleRepo.CheckIfExist(a => a.Id != role.Id && a.Name.Trim().ToLower().Equals(name.Trim().ToLower())))
            throw new Exception($"User with name `{name}` already exists");
    }

    public async Task ValidateRoleUpdate(Role role)
    {
        if (!role.IsGlobal) await _userProvider.ValidateBranchUsage(role.BranchId);
        else if (!await _userProvider.IsMainBranch()) throw new Exception("Error: Access Denied. You cannot edit a global role");
    }
}