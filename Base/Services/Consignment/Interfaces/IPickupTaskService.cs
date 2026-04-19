using Base.Dtos.Consignment;
using Base.Entities.Consignment;

namespace Base.Services.Consignment.Interfaces;

public interface IPickupTaskService
{
    Task<PickupTask> Create(PickupTaskCreateDto dto);
    Task Assign(PickupAssignDto dto);
    Task BulkAssign(PickupBulkAssignDto dto);
    Task MarkInProgress(long pickupTaskId);
    Task Complete(PickupCompleteDto dto);
    Task Fail(PickupFailDto dto);
    Task Cancel(long pickupTaskId, string? reason = null);
}
