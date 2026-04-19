using Base.Dtos;
using Base.Entities;

namespace Base.Validator.Interface;

public interface IVehicleAssignmentValidator
{
    Task ValidateCreate(VehicleAssignmentAddDto dto);
    Task ValidateUpdate(VehicleAssignment assignment, VehicleAssignmentUpdateDto dto);
}
