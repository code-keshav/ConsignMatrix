using Base.Dtos;
using Base.Entities;

namespace Base.Services.Interfaces;

public interface IVehicleService
{
    Task<Vehicle> Create(VehicleAddDto dto);
    Task Update(Vehicle vehicle, VehicleUpdateDto dto);
    Task Delete(Vehicle vehicle);
}
