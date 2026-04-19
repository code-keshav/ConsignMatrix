using Base.Entities;
using Base.Repo.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Base.Repo;

public class BranchPinCodeRepo : GenericRepo<BranchPinCode>, IBranchPinCodeRepo
{
    public BranchPinCodeRepo(DbContext context) : base(context)
    {
    }

    public async Task<List<BranchPinCode>> GetByBranchId(long branchId)
    {
        return await GetQueryable()
            .Where(p => p.BranchId == branchId && p.IsActive)
            .ToListAsync();
    }

    public async Task<List<Branch>> GetBranchesServingPinCode(string pinCode)
    {
        return await GetQueryable()
            .Where(p => p.PinCode == pinCode && p.IsActive)
            .Select(p => p.Branch)
            .Where(b => b.Status == StatusEnum.Active)
            .ToListAsync();
    }
}
