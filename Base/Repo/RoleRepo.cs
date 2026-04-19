using Base.Entities;
using Base.Repo.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Base.Repo;

public class RoleRepo : GenericRepo<Role>, IRoleRepo
{
    public RoleRepo(DbContext context) : base(context)
    {
    }
    
    public async Task<List<Role>> GetRoles()
    {
        return await GetQueryable().ToListAsync();
    }
}