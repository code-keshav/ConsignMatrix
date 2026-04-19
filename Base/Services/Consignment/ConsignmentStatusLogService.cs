using System.Transactions;
using Base.Entities.Consignment;
using Base.Enum.Consignment;
using Base.Repo.Interfaces;
using Base.Services.Consignment.Interfaces;

namespace Base.Services.Consignment;

public class ConsignmentStatusLogService : IConsignmentStatusLogService
{
    private readonly IUow _uow;

    public ConsignmentStatusLogService(IUow uow)
    {
        _uow = uow;
    }

    public async Task AddStatus(long consignmentId, ConsignmentStatusType statusType, string? remarks = null)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var log = new ConsignmentStatusLog
        {
            ConsignmentId = consignmentId,
            StatusType = statusType,
            Remarks = remarks,
        };
        await _uow.CreateAsync(log);
        await _uow.CommitAsync();
        tx.Complete();
    }
}
