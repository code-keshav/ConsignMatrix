using Base.Entities.Consignment;
using Base.Repo.Interfaces;

namespace Base.Repo.Consignment.Interfaces;

public interface IPickupTaskRepo : IGenericRepo<PickupTask>
{
    Task<PickupTask?> GetActiveByConsignmentId(long consignmentId);
    Task<List<PickupTask>> GetByConsignmentId(long consignmentId);
}
