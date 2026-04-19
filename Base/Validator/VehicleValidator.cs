using Base.Entities;
using Base.Repo.Interfaces;
using Base.Validator.Interface;

namespace Base.Validator;

public class VehicleValidator : IVehicleValidator
{
    private readonly IVehicleRepo _vehicleRepo;

    public VehicleValidator(IVehicleRepo vehicleRepo)
    {
        _vehicleRepo = vehicleRepo;
    }

    public void ValidateVehicleNumber(Vehicle vehicle, string vehicleNumber)
    {
        if (_vehicleRepo.CheckIfExist(a => a.VehicleNumber == vehicleNumber && a.Id != vehicle.Id))
            throw new Exception($"Vehicle with number `{vehicleNumber}` already exists");
    }

    public void ValidateInsuranceExpiry(DateTime insuranceExpiry)
    {
        if (insuranceExpiry.Date < DateTime.UtcNow.Date)
            throw new Exception("Insurance expiry date cannot be in the past");
    }
}
