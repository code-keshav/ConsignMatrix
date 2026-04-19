using Base.Entities.Consignment;
using Base.Repo.Interfaces;

namespace Base.Repo.Consignment.Interfaces;

public interface ITripConsignmentRepo : IGenericRepo<TripConsignment>
{
    Task<List<TripConsignment>> GetByTripId(long tripId);
    Task<bool> IsInActiveTripAsync(long consignmentId);
}
