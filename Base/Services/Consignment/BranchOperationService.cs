using System.Transactions;
using Base.Dtos.Consignment;
using Base.Enum.Consignment;
using Base.Repo.Consignment.Interfaces;
using Base.Repo.Interfaces;
using Base.Services.Consignment.Interfaces;

namespace Base.Services.Consignment;

public class BranchOperationService : IBranchOperationService
{
    private readonly IUow _uow;
    private readonly IConsignmentRepo _consignmentRepo;
    private readonly IConsignmentStatusLogRepo _statusLogRepo;
    private readonly IConsignmentStatusLogService _statusLogService;

    public BranchOperationService(IUow uow, IConsignmentRepo consignmentRepo,
        IConsignmentStatusLogRepo statusLogRepo, IConsignmentStatusLogService statusLogService)
    {
        _uow = uow;
        _consignmentRepo = consignmentRepo;
        _statusLogRepo = statusLogRepo;
        _statusLogService = statusLogService;
    }

    public async Task ReceiveConsignment(ReceiveConsignmentDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var consignment = await _consignmentRepo.FindOrThrowAsync(dto.ConsignmentId);
        var latest = await _statusLogRepo.GetLatestStatus(dto.ConsignmentId);

        if (latest?.StatusType != ConsignmentStatusType.Booked &&
            latest?.StatusType != ConsignmentStatusType.PickedUp)
            throw new Exception("Consignment must be in Booked or PickedUp status to be received at origin.");

        await _statusLogService.AddStatus(dto.ConsignmentId, ConsignmentStatusType.ReceivedAtOrigin,
            dto.Remarks ?? $"Received at origin branch — {consignment.TrackingNumber}");

        tx.Complete();
    }

    public async Task BulkReceive(BulkReceiveDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        if (dto.ConsignmentIds == null || dto.ConsignmentIds.Count == 0)
            throw new Exception("At least one consignment must be selected.");

        foreach (var id in dto.ConsignmentIds)
        {
            var consignment = await _consignmentRepo.FindOrThrowAsync(id);
            var latest = await _statusLogRepo.GetLatestStatus(id);

            if (latest?.StatusType != ConsignmentStatusType.Booked &&
                latest?.StatusType != ConsignmentStatusType.PickedUp)
                throw new Exception(
                    $"Consignment {consignment.TrackingNumber} is not in Booked or PickedUp status.");

            await _statusLogService.AddStatus(id, ConsignmentStatusType.ReceivedAtOrigin,
                dto.Remarks ?? $"Received at origin branch — {consignment.TrackingNumber}");
        }

        tx.Complete();
    }

    public async Task SortConsignment(SortConsignmentDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var consignment = await _consignmentRepo.FindOrThrowAsync(dto.ConsignmentId);
        var latest = await _statusLogRepo.GetLatestStatus(dto.ConsignmentId);

        if (latest?.StatusType != ConsignmentStatusType.ReceivedAtOrigin)
            throw new Exception("Consignment must be in ReceivedAtOrigin status to be sorted.");

        if (dto.DestinationBranchId > 0 && dto.DestinationBranchId != consignment.DestinationBranchId)
        {
            consignment.DestinationBranchId = dto.DestinationBranchId;
            _uow.Update(consignment);
            await _uow.CommitAsync();
        }

        await _statusLogService.AddStatus(dto.ConsignmentId, ConsignmentStatusType.Sorted,
            dto.Remarks ?? $"Sorted for destination branch #{dto.DestinationBranchId}");

        tx.Complete();
    }

    public async Task BulkSort(BulkSortDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        if (dto.Items == null || dto.Items.Count == 0)
            throw new Exception("At least one consignment must be selected.");

        foreach (var item in dto.Items)
        {
            var consignment = await _consignmentRepo.FindOrThrowAsync(item.ConsignmentId);
            var latest = await _statusLogRepo.GetLatestStatus(item.ConsignmentId);

            if (latest?.StatusType != ConsignmentStatusType.ReceivedAtOrigin)
                throw new Exception(
                    $"Consignment {consignment.TrackingNumber} is not in ReceivedAtOrigin status.");

            if (item.DestinationBranchId > 0 && item.DestinationBranchId != consignment.DestinationBranchId)
            {
                consignment.DestinationBranchId = item.DestinationBranchId;
                _uow.Update(consignment);
                await _uow.CommitAsync();
            }

            await _statusLogService.AddStatus(item.ConsignmentId, ConsignmentStatusType.Sorted,
                dto.Remarks ?? $"Sorted for destination branch #{item.DestinationBranchId}");
        }

        tx.Complete();
    }

    public async Task BagConsignment(BagConsignmentDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var consignment = await _consignmentRepo.FindOrThrowAsync(dto.ConsignmentId);
        var latest = await _statusLogRepo.GetLatestStatus(dto.ConsignmentId);

        if (latest?.StatusType != ConsignmentStatusType.Sorted)
            throw new Exception("Consignment must be in Sorted status to be bagged.");

        await _statusLogService.AddStatus(dto.ConsignmentId, ConsignmentStatusType.Bagged,
            dto.Remarks ?? $"Bagged for dispatch — {consignment.TrackingNumber}");

        tx.Complete();
    }

    public async Task BulkBag(BulkBagDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        if (dto.ConsignmentIds == null || dto.ConsignmentIds.Count == 0)
            throw new Exception("At least one consignment must be selected.");

        foreach (var id in dto.ConsignmentIds)
        {
            var consignment = await _consignmentRepo.FindOrThrowAsync(id);
            var latest = await _statusLogRepo.GetLatestStatus(id);

            if (latest?.StatusType != ConsignmentStatusType.Sorted)
                throw new Exception(
                    $"Consignment {consignment.TrackingNumber} is not in Sorted status.");

            await _statusLogService.AddStatus(id, ConsignmentStatusType.Bagged,
                dto.Remarks ?? $"Bagged for dispatch — {consignment.TrackingNumber}");
        }

        tx.Complete();
    }
}
