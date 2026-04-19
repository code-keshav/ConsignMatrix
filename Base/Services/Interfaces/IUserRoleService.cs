using Base.Dtos;
using Base.Entities;

namespace Base.Services.Interfaces;

public interface IUserRoleService
{
    public Task AssignRole(UserRoleDto dto);
    Task UnassignRole(UserRole userRole);
    Task AssignRole(List<UserRoleDto> rlDtos);
}