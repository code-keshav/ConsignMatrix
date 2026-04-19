using Base.Entities.Consignment;
using Base.Enum.Consignment;
using Base.Repo.Consignment.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Base.Repo.Consignment;

public class PickupTaskRepo : GenericRepo<PickupTask>, IPickupTaskRepo
{
    public PickupTaskRepo(DbContext context) : base(context)
    {
    }

    public Task<PickupTask?> GetActiveByConsignmentId(long consignmentId) =>
        GetQueryable()
            .Where(p => p.ConsignmentId == consignmentId
                        && p.TaskStatus != PickupTaskStatus.Completed
                        && p.TaskStatus != PickupTaskStatus.Failed
                        && p.TaskStatus != PickupTaskStatus.Cancelled)
            .FirstOrDefaultAsync();

    public Task<List<PickupTask>> GetByConsignmentId(long consignmentId) =>
        GetQueryable()
            .Where(p => p.ConsignmentId == consignmentId)
            .OrderByDescending(p => p.RecDate)
            .ToListAsync();
}
