using Base.Dtos;
using Base.Entities;

namespace Base.Services.Interfaces;

public interface IVehicleAssignmentService
{
    Task CreateBulk(long vehicleId, List<VehicleAssignmentAddDto> dtos);
    Task Update(VehicleAssignment assignment, VehicleAssignmentUpdateDto dto);
    Task Delete(VehicleAssignment assignment);
    Task Deactivate(VehicleAssignment assignment);
}
