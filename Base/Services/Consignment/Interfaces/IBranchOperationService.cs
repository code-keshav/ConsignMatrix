using Base.Dtos.Consignment;

namespace Base.Services.Consignment.Interfaces;

public interface IBranchOperationService
{
    Task ReceiveConsignment(ReceiveConsignmentDto dto);
    Task BulkReceive(BulkReceiveDto dto);
    Task SortConsignment(SortConsignmentDto dto);
    Task BulkSort(BulkSortDto dto);
    Task BagConsignment(BagConsignmentDto dto);
    Task BulkBag(BulkBagDto dto);
}
