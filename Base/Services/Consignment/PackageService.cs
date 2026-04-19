using System.Transactions;
using Base.Dtos.Consignment;
using Base.Entities.Consignment;
using Base.Enum.Consignment;
using Base.Repo.Consignment.Interfaces;
using Base.Repo.Interfaces;
using Base.Services.Consignment.Interfaces;

namespace Base.Services.Consignment;

public class PackageService : IPackageService
{
    private readonly IUow _uow;
    private readonly IConsignmentRepo _consignmentRepo;
    private readonly IPackageRepo _packageRepo;
    private readonly IConsignmentStatusLogRepo _statusLogRepo;

    public PackageService(IUow uow, IConsignmentRepo consignmentRepo, IPackageRepo packageRepo,
        IConsignmentStatusLogRepo statusLogRepo)
    {
        _uow = uow;
        _consignmentRepo = consignmentRepo;
        _packageRepo = packageRepo;
        _statusLogRepo = statusLogRepo;
    }

    public async Task Create(long consignmentId, PackageDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var consignment = await _consignmentRepo.FindOrThrowAsync(consignmentId);

        await ValidateConsignmentEditable(consignmentId);

        var packageNumber = await _packageRepo.GetNextPackageNumber(consignmentId);
        var volume = dto.Length * dto.Width * dto.Height;
        var volumetricWeight = volume / 5000m;

        var package = new Package
        {
            ConsignmentId = consignmentId,
            PackageNumber = packageNumber,
            Barcode = $"PKG{consignmentId}-{packageNumber}",
            Weight = dto.Weight,
            Length = dto.Length,
            Width = dto.Width,
            Height = dto.Height,
            Volume = volume,
            VolumetricWeight = volumetricWeight,
            PackageType = dto.PackageType,
            ContentDescription = dto.ContentDescription,
            IsFragile = dto.IsFragile,
            IsHazardous = dto.IsHazardous,
            CanBeStacked = dto.CanBeStacked,
            RequiresColdChain = dto.RequiresColdChain,
        };
        await _uow.CreateAsync(package);
        await _uow.CommitAsync();

        await RecalculateWeights(consignment);
        tx.Complete();
    }

    public async Task Update(Package package, PackageDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        await ValidateConsignmentEditable(package.ConsignmentId);

        package.Weight = dto.Weight;
        package.Length = dto.Length;
        package.Width = dto.Width;
        package.Height = dto.Height;
        package.Volume = dto.Length * dto.Width * dto.Height;
        package.VolumetricWeight = package.Volume / 5000m;
        package.PackageType = dto.PackageType;
        package.ContentDescription = dto.ContentDescription;
        package.IsFragile = dto.IsFragile;
        package.IsHazardous = dto.IsHazardous;
        package.CanBeStacked = dto.CanBeStacked;
        package.RequiresColdChain = dto.RequiresColdChain;
        _uow.Update(package);
        await _uow.CommitAsync();

        var consignment = await _consignmentRepo.FindOrThrowAsync(package.ConsignmentId);
        await RecalculateWeights(consignment);
        tx.Complete();
    }

    public async Task Delete(Package package)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var consignmentId = package.ConsignmentId;

        await ValidateConsignmentEditable(consignmentId);

        // Prevent deleting the last package
        var packages = await _packageRepo.GetByConsignmentId(consignmentId);
        if (packages.Count <= 1)
            throw new Exception("Cannot delete the last package — a consignment must have at least one package.");

        _uow.Remove(package);
        await _uow.CommitAsync();

        var consignment = await _consignmentRepo.FindOrThrowAsync(consignmentId);
        await RecalculateWeights(consignment);
        tx.Complete();
    }

    private async Task ValidateConsignmentEditable(long consignmentId)
    {
        var latest = await _statusLogRepo.GetLatestStatus(consignmentId);
        if (latest?.StatusType != ConsignmentStatusType.Booked &&
            latest?.StatusType != ConsignmentStatusType.PickupScheduled)
            throw new Exception("Packages can only be modified while the consignment is in Booked or PickupScheduled status.");
    }

    private async Task RecalculateWeights(Entities.Consignment.Consignment consignment)
    {
        var packages = await _packageRepo.GetByConsignmentId(consignment.Id);
        consignment.TotalWeight = packages.Sum(p => p.Weight);
        consignment.VolumetricWeight = packages.Sum(p => p.VolumetricWeight);
        consignment.ChargeableWeight = Math.Max(consignment.TotalWeight, consignment.VolumetricWeight);
        consignment.TotalVolume = packages.Sum(p => p.Volume);
        consignment.PackageCount = packages.Count;
        _uow.Update(consignment);
        await _uow.CommitAsync();
    }
}
