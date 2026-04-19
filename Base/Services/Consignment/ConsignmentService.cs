using System.Transactions;
using Base.Dtos.Consignment;
using Base.Entities.Consignment;
using Base.Enum.Consignment;
using Base.Repo.Consignment.Interfaces;
using Base.Repo.Interfaces;
using Base.Services.Consignment.Interfaces;
using ConsignmentEntity = Base.Entities.Consignment.Consignment;

namespace Base.Services.Consignment;

public class ConsignmentService : IConsignmentService
{
    private readonly IUow _uow;
    private readonly IConsignmentRepo _consignmentRepo;
    private readonly IPackageRepo _packageRepo;
    private readonly ICustomerService _customerService;
    private readonly ICustomerAddressService _customerAddressService;
    private readonly IConsignmentStatusLogService _statusLogService;
    private readonly IConsignmentStatusLogRepo _statusLogRepo;
    private readonly IBranchPinCodeRepo _branchPinCodeRepo;

    public ConsignmentService(IUow uow, IConsignmentRepo consignmentRepo, IPackageRepo packageRepo,
        ICustomerService customerService, ICustomerAddressService customerAddressService,
        IConsignmentStatusLogService statusLogService, IConsignmentStatusLogRepo statusLogRepo,
        IBranchPinCodeRepo branchPinCodeRepo)
    {
        _uow = uow;
        _consignmentRepo = consignmentRepo;
        _packageRepo = packageRepo;
        _customerService = customerService;
        _customerAddressService = customerAddressService;
        _statusLogService = statusLogService;
        _statusLogRepo = statusLogRepo;
        _branchPinCodeRepo = branchPinCodeRepo;
    }

    public async Task Create(ConsignmentDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        // Validate packages
        if (dto.Packages == null || dto.Packages.Count == 0)
            throw new Exception("At least one package is required.");
        if (dto.Packages.Any(p => p.Weight <= 0))
            throw new Exception("All packages must have a weight greater than 0.");
        if (dto.Packages.Any(p => p.Length <= 0 || p.Width <= 0 || p.Height <= 0))
            throw new Exception("All packages must have dimensions greater than 0.");

        // Validate destination branch
        if (dto.DestinationBranchId <= 0)
            throw new Exception("Destination branch is required.");

        // Validate COD amount
        if (dto.PaymentMode == PaymentMode.COD && (dto.CodAmount == null || dto.CodAmount <= 0))
            throw new Exception("COD amount is required and must be greater than 0 for COD payment mode.");

        // Resolve sender
        long senderId;
        long senderAddressId;
        if (dto.SenderId.HasValue && dto.SenderAddressId.HasValue)
        {
            senderId = dto.SenderId.Value;
            senderAddressId = dto.SenderAddressId.Value;
        }
        else if (dto.SenderId.HasValue && dto.NewSenderAddress != null)
        {
            senderId = dto.SenderId.Value;
            dto.NewSenderAddress.CustomerId = senderId;
            var address = await _customerAddressService.Create(dto.NewSenderAddress);
            senderAddressId = address.Id;
        }
        else if (dto.NewSender != null)
        {
            var (customerId, addressId) = await CreateCustomerWithAddress(dto.NewSender);
            senderId = customerId;
            senderAddressId = addressId;
        }
        else
        {
            throw new Exception("Sender information is required.");
        }

        // Resolve receiver
        long receiverId;
        long receiverAddressId;
        if (dto.ReceiverId.HasValue && dto.ReceiverAddressId.HasValue)
        {
            receiverId = dto.ReceiverId.Value;
            receiverAddressId = dto.ReceiverAddressId.Value;
        }
        else if (dto.ReceiverId.HasValue && dto.NewReceiverAddress != null)
        {
            receiverId = dto.ReceiverId.Value;
            dto.NewReceiverAddress.CustomerId = receiverId;
            var address = await _customerAddressService.Create(dto.NewReceiverAddress);
            receiverAddressId = address.Id;
        }
        else if (dto.NewReceiver != null)
        {
            var (customerId, addressId) = await CreateCustomerWithAddress(dto.NewReceiver);
            receiverId = customerId;
            receiverAddressId = addressId;
        }
        else
        {
            throw new Exception("Receiver information is required.");
        }

        // Generate tracking number
        var trackingNumber = await _consignmentRepo.GenerateTrackingNumber();

        // Create consignment
        var consignment = new ConsignmentEntity
        {
            TrackingNumber = trackingNumber,
            SenderId = senderId,
            SenderAddressId = senderAddressId,
            ReceiverId = receiverId,
            ReceiverAddressId = receiverAddressId,
            OriginBranchId = dto.BranchId, // Will be set by BaseEntity audit (RecBranchId)
            DestinationBranchId = dto.DestinationBranchId,
            ServiceType = dto.ServiceType,
            PaymentMode = dto.PaymentMode,
            DeclaredValue = dto.DeclaredValue,
            CodAmount = dto.CodAmount,
            SpecialInstructions = dto.SpecialInstructions,
        };
        await _uow.CreateAsync(consignment);
        await _uow.CommitAsync();

        // Set OriginBranchId from audit field
        if (consignment.OriginBranchId == 0)
        {
            consignment.OriginBranchId = consignment.RecBranchId;
            _uow.Update(consignment);
            await _uow.CommitAsync();
        }

        // Create packages
        var packageNumber = 0;
        foreach (var pkgDto in dto.Packages)
        {
            packageNumber++;
            var volume = pkgDto.Length * pkgDto.Width * pkgDto.Height;
            var volumetricWeight = volume / 5000m;
            var package = new Package
            {
                ConsignmentId = consignment.Id,
                PackageNumber = packageNumber,
                Barcode = $"PKG{consignment.Id}-{packageNumber}",
                Weight = pkgDto.Weight,
                Length = pkgDto.Length,
                Width = pkgDto.Width,
                Height = pkgDto.Height,
                Volume = volume,
                VolumetricWeight = volumetricWeight,
                PackageType = pkgDto.PackageType,
                ContentDescription = pkgDto.ContentDescription,
                IsFragile = pkgDto.IsFragile,
                IsHazardous = pkgDto.IsHazardous,
                CanBeStacked = pkgDto.CanBeStacked,
                RequiresColdChain = pkgDto.RequiresColdChain,
            };
            await _uow.CreateAsync(package);
        }

        await _uow.CommitAsync();

        // Recalculate weights
        await RecalculateWeights(consignment.Id);

        // Create initial status log
        await _statusLogService.AddStatus(consignment.Id, ConsignmentStatusType.Booked, "Consignment booked");

        tx.Complete();
    }

    public async Task Update(ConsignmentEntity consignment, ConsignmentUpdateDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        // Only allow editing when consignment is in Booked or PickupScheduled status
        var latest = await _statusLogRepo.GetLatestStatus(consignment.Id);
        if (latest?.StatusType != ConsignmentStatusType.Booked &&
            latest?.StatusType != ConsignmentStatusType.PickupScheduled)
            throw new Exception("Consignment can only be edited while in Booked or PickupScheduled status.");

        // Validate COD amount
        if (dto.PaymentMode == PaymentMode.COD && (dto.CodAmount == null || dto.CodAmount <= 0))
            throw new Exception("COD amount is required and must be greater than 0 for COD payment mode.");

        consignment.ServiceType = dto.ServiceType;
        consignment.PaymentMode = dto.PaymentMode;
        consignment.DeclaredValue = dto.DeclaredValue;
        consignment.CodAmount = dto.CodAmount;
        consignment.SpecialInstructions = dto.SpecialInstructions;

        if (dto.DestinationBranchId.HasValue && dto.DestinationBranchId.Value > 0
            && dto.DestinationBranchId.Value != consignment.DestinationBranchId)
        {
            consignment.DestinationBranchId = dto.DestinationBranchId.Value;
        }

        _uow.Update(consignment);
        await _uow.CommitAsync();
        tx.Complete();
    }

    public async Task Delete(ConsignmentEntity consignment)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        // Only allow deletion when consignment is in Booked status
        var latest = await _statusLogRepo.GetLatestStatus(consignment.Id);
        if (latest?.StatusType != ConsignmentStatusType.Booked)
            throw new Exception("Only consignments in Booked status can be deleted.");

        _uow.Remove(consignment);
        await _uow.CommitAsync();
        tx.Complete();
    }

    public async Task Activate(ConsignmentEntity consignment)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        consignment.MarkAsActive();
        _uow.Update(consignment);
        await _uow.CommitAsync();
        tx.Complete();
    }

    public async Task Deactivate(ConsignmentEntity consignment)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        // Only allow deactivation when consignment is in Booked status
        var latest = await _statusLogRepo.GetLatestStatus(consignment.Id);
        if (latest?.StatusType != ConsignmentStatusType.Booked)
            throw new Exception("Only consignments in Booked status can be deactivated.");

        consignment.MarkAsInactive();
        _uow.Update(consignment);
        await _uow.CommitAsync();
        tx.Complete();
    }

    private async Task RecalculateWeights(long consignmentId)
    {
        var consignment = await _consignmentRepo.FindOrThrowAsync(consignmentId);
        var packages = await _packageRepo.GetByConsignmentId(consignmentId);
        consignment.TotalWeight = packages.Sum(p => p.Weight);
        consignment.VolumetricWeight = packages.Sum(p => p.VolumetricWeight);
        consignment.ChargeableWeight = Math.Max(consignment.TotalWeight, consignment.VolumetricWeight);
        consignment.TotalVolume = packages.Sum(p => p.Volume);
        consignment.PackageCount = packages.Count;
        _uow.Update(consignment);
        await _uow.CommitAsync();
    }

    private async Task<(long CustomerId, long AddressId)> CreateCustomerWithAddress(CustomerDto dto)
    {
        await _customerService.Create(dto);
        var customerRepo = _uow.Repo<ICustomerRepo>();
        var customer = await customerRepo
            .FindSingleOrThrowAsync(c => c.PhoneNo == dto.PhoneNo && c.Name == dto.Name);
        var address = customer.CustomerAddresses.First();
        return (customer.Id, address.Id);
    }
}
