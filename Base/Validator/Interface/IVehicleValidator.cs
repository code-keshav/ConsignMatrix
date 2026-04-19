using Base.Entities;

namespace Base.Validator.Interface;

public interface IVehicleValidator
{
    void ValidateVehicleNumber(Vehicle vehicle, string vehicleNumber);
    void ValidateInsuranceExpiry(DateTime insuranceExpiry);
}
