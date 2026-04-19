using Base.Repo.Consignment.Interfaces;
using Microsoft.EntityFrameworkCore;
using ConsignmentEntity = Base.Entities.Consignment.Consignment;

namespace Base.Repo.Consignment;

public class ConsignmentRepo : GenericRepo<ConsignmentEntity>, IConsignmentRepo
{
    public ConsignmentRepo(DbContext context) : base(context)
    {
    }

    public async Task<string> GenerateTrackingNumber()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var count = await GetQueryable()
            .CountAsync(c => c.RecDate >= today && c.RecDate < tomorrow);
        var sequence = (count + 1).ToString("D5");
        return $"TRK{today:yyyyMMdd}{sequence}";
    }
}
