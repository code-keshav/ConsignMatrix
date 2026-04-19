using Base.Repo.Interfaces;
using ConsignmentEntity = Base.Entities.Consignment.Consignment;

namespace Base.Repo.Consignment.Interfaces;

public interface IConsignmentRepo : IGenericRepo<ConsignmentEntity>
{
    Task<string> GenerateTrackingNumber();
}
