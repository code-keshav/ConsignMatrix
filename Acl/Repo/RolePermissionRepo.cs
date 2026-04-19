using Acl.Entities;
using Acl.Repo.Interfaces;
using Base.Constants;
using Base.Repo;
using Microsoft.EntityFrameworkCore;

namespace Acl.Repo;

public class RolePermissionRepo : GenericRepo<RolePermission>, IRolePermissionRepo
{
    public RolePermissionRepo(DbContext context) : base(context)
    {
    }

    public async Task<List<string>> GetPermissions(long roleId, long branchId)
    {

        var list = await GetQueryable().Where(x => (x.BranchId == branchId || x.BranchId == (long)IdConstants.MainBranchId) &&
                   x.RoleId == roleId).Select(x => x.Permission).ToListAsync();
        return list;
    }
}