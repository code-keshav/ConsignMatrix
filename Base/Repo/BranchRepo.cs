using Base.Constants;
using Base.Entities;
using Base.Repo.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Base.Repo;

public class BranchRepo : GenericRepo<Branch>, IBranchRepo
{
    public BranchRepo(DbContext context) : base(context)
    {
    }

    public async Task<Branch> GetMainBranch()
    {
        return await GetQueryable().FirstAsync(a => a.Id == (long)IdConstants.MainBranchId);
    }
}