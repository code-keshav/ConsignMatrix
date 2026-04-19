using Base.Entities.Consignment;
using Base.Repo.Consignment.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Base.Repo.Consignment;

public class TripRepo : GenericRepo<Trip>, ITripRepo
{
    public TripRepo(DbContext context) : base(context)
    {
    }

    public async Task<string> GenerateTripNumber()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var count = await GetQueryable()
            .CountAsync(t => t.RecDate >= today && t.RecDate < tomorrow);
        var sequence = (count + 1).ToString("D4");
        return $"TRP{today:yyyyMMdd}{sequence}";
    }

    public async Task<Trip?> GetWithConsignments(long id)
    {
        return await GetQueryable()
            .Where(t => t.Id == id)
            .FirstOrDefaultAsync();
    }
}
