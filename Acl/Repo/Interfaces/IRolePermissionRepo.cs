using Acl.Entities;
using Base.Repo.Interfaces;

namespace Acl.Repo.Interfaces;

public interface IRolePermissionRepo : IGenericRepo<RolePermission>
{
    Task<List<string>> GetPermissions(long roleId, long branchId);

}