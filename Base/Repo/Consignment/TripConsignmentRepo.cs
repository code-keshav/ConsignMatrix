using Base.Entities.Consignment;
using Base.Enum.Consignment;
using Base.Repo.Consignment.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Base.Repo.Consignment;

public class TripConsignmentRepo : GenericRepo<TripConsignment>, ITripConsignmentRepo
{
    public TripConsignmentRepo(DbContext context) : base(context)
    {
    }

    public async Task<List<TripConsignment>> GetByTripId(long tripId)
        => await GetQueryable()
            .Where(tc => tc.TripId == tripId)
            .OrderBy(tc => tc.LoadedAt)
            .ToListAsync();

    public async Task<bool> IsInActiveTripAsync(long consignmentId)
        => await GetQueryable()
            .AnyAsync(tc => tc.ConsignmentId == consignmentId
                && (tc.Trip.TripStatus == TripStatus.Scheduled || tc.Trip.TripStatus == TripStatus.InTransit));
}
