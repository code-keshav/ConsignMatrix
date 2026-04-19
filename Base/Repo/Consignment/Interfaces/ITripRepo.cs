using Base.Entities.Consignment;
using Base.Repo.Interfaces;

namespace Base.Repo.Consignment.Interfaces;

public interface ITripRepo : IGenericRepo<Trip>
{
    Task<string> GenerateTripNumber();
    Task<Trip?> GetWithConsignments(long id);
}
