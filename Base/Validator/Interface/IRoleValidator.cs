using Base.Entities;

namespace Base.Validator.Interface;

public interface IRoleValidator
{
    void ValidateRoleUse(Role role);
    void ValidateRoleName(Role role, string name);
    Task ValidateRoleUpdate(Role role);
}