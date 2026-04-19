using System.Transactions;
using Base.Dtos;
using Base.Entities;
using Base.Enum;
using Base.Repo.Interfaces;
using Base.Services.Interfaces;
using Base.Validator.Interface;

namespace Base.Services;

public class VehicleService : IVehicleService
{
    private readonly IUow _uow;
    private readonly IVehicleRepo _vehicleRepo;
    private readonly IVehicleValidator _vehicleValidator;

    public VehicleService(IUow uow, IVehicleRepo vehicleRepo, IVehicleValidator vehicleValidator)
    {
        _uow = uow;
        _vehicleRepo = vehicleRepo;
        _vehicleValidator = vehicleValidator;
    }

    public async Task<Vehicle> Create(VehicleAddDto dto)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var vehicle = new Vehicle
        {
            VehicleNumber = dto.VehicleNumber,
            VehicleType = dto.VehicleType,
            OwnershipType = dto.OwnershipType,
            MaxWeightCapacity = dto.MaxWeightCapacity,
            MaxVolumeCapacity = dto.MaxVolumeCapacity,
            SupportsFragile = dto.SupportsFragile,
            HasColdStorage = dto.HasColdStorage,
            VehicleStatus = dto.VehicleStatus,
            LastServiceDate = dto.LastServiceDate,
            InsuranceExpiry = dto.InsuranceExpiry,
            FuelType = dto.FuelType,
            CurrentBranchId = dto.CurrentBranchId
        };
        _vehicleValidator.ValidateVehicleNumber(vehicle, dto.VehicleNumber);
        _vehicleValidator.ValidateInsuranceExpiry(dto.InsuranceExpiry);
        await _uow.CreateAsync(vehicle);
        await _uow.CommitAsync();
        scope.Complete();
        return vehicle;
    }

    public async Task Update(Vehicle vehicle, VehicleUpdateDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        _vehicleValidator.ValidateVehicleNumber(vehicle, dto.VehicleNumber);
        vehicle.VehicleNumber = dto.VehicleNumber;
        vehicle.VehicleType = dto.VehicleType;
        vehicle.OwnershipType = dto.OwnershipType;
        vehicle.MaxWeightCapacity = dto.MaxWeightCapacity;
        vehicle.MaxVolumeCapacity = dto.MaxVolumeCapacity;
        vehicle.SupportsFragile = dto.SupportsFragile;
        vehicle.HasColdStorage = dto.HasColdStorage;
        vehicle.VehicleStatus = dto.VehicleStatus;
        vehicle.LastServiceDate = dto.LastServiceDate;
        vehicle.InsuranceExpiry = dto.InsuranceExpiry;
        vehicle.FuelType = dto.FuelType;
        vehicle.CurrentBranchId = dto.CurrentBranchId;
        _uow.Update(vehicle);
        await _uow.CommitAsync();
        tx.Complete();
    }

    public async Task Delete(Vehicle vehicle)
    {
        if (vehicle.VehicleStatus == VehicleStatus.OnTrip)
            throw new Exception("Cannot delete a vehicle that is currently on a trip");

        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        _uow.Remove(vehicle);
        await _uow.CommitAsync();
        tx.Complete();
    }
}
