using Base.Enum.Consignment;

namespace Base.Services.Consignment.Interfaces;

public interface IConsignmentStatusLogService
{
    Task AddStatus(long consignmentId, ConsignmentStatusType statusType, string? remarks = null);
}
