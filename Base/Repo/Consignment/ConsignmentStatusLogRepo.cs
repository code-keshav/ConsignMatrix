using Base.Entities.Consignment;
using Base.Repo.Consignment.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Base.Repo.Consignment;

public class ConsignmentStatusLogRepo : GenericRepo<ConsignmentStatusLog>, IConsignmentStatusLogRepo
{
    public ConsignmentStatusLogRepo(DbContext context) : base(context)
    {
    }

    public Task<List<ConsignmentStatusLog>> GetByConsignmentId(long consignmentId) =>
        GetQueryable()
            .Where(s => s.ConsignmentId == consignmentId)
            .OrderByDescending(s => s.RecDate)
            .ToListAsync();

    public Task<ConsignmentStatusLog?> GetLatestStatus(long consignmentId) =>
        GetQueryable()
            .Where(s => s.ConsignmentId == consignmentId)
            .OrderByDescending(s => s.RecDate)
            .FirstOrDefaultAsync();
}
