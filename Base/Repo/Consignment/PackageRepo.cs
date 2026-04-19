using Base.Entities.Consignment;
using Base.Repo.Consignment.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Base.Repo.Consignment;

public class PackageRepo : GenericRepo<Package>, IPackageRepo
{
    public PackageRepo(DbContext context) : base(context)
    {
    }

    public Task<List<Package>> GetByConsignmentId(long consignmentId) =>
        GetQueryable().Where(p => p.ConsignmentId == consignmentId).ToListAsync();

    public async Task<int> GetNextPackageNumber(long consignmentId)
    {
        var max = await GetQueryable()
            .Where(p => p.ConsignmentId == consignmentId)
            .MaxAsync(p => (int?)p.PackageNumber);
        return (max ?? 0) + 1;
    }
}
