using Base.Dtos;
using Base.Entities;

namespace Base.Services.Interfaces;

public interface IRoleService
{
    Task Create(RoleDto dto);
    Task Update(Role role, RoleEditDto dto);
    Task Delete(Role role);
    Task MarkAsGlobal(Role role, Branch branch);
    Task UnmarkAsGlobal(Role role, Branch branch);
}