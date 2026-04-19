using Acl.Dtos;

namespace Acl.Services.Interfaces;

public interface IRolePermissionService
{
    Task UpdatePermission(List<RolePermissionDto> permissionDtos);
}