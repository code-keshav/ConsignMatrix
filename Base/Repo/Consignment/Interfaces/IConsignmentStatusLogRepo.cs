using Base.Entities.Consignment;
using Base.Repo.Interfaces;

namespace Base.Repo.Consignment.Interfaces;

public interface IConsignmentStatusLogRepo : IGenericRepo<ConsignmentStatusLog>
{
    Task<List<ConsignmentStatusLog>> GetByConsignmentId(long consignmentId);
    Task<ConsignmentStatusLog?> GetLatestStatus(long consignmentId);
}
