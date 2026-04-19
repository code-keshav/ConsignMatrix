using Base.Entities.Consignment;
using Base.Repo.Interfaces;

namespace Base.Repo.Consignment.Interfaces;

public interface IPackageRepo : IGenericRepo<Package>
{
    Task<List<Package>> GetByConsignmentId(long consignmentId);
    Task<int> GetNextPackageNumber(long consignmentId);
}
