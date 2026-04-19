using Base.Constants;
using Base.Dtos;
using Base.Entities;
using Base.Enum;
using Base.Repo.Interfaces;
using Base.Validator.Interface;
using Microsoft.EntityFrameworkCore;

namespace Base.Validator;

public class VehicleAssignmentValidator : IVehicleAssignmentValidator
{
    private readonly IVehicleRepo _vehicleRepo;
    private readonly IEmployeeRepo _employeeRepo;
    private readonly IDriverRepo _driverRepo;
    private readonly IVehicleAssignmentRepo _vehicleAssignmentRepo;

    public VehicleAssignmentValidator(IVehicleRepo vehicleRepo, IEmployeeRepo employeeRepo,
        IDriverRepo driverRepo, IVehicleAssignmentRepo vehicleAssignmentRepo)
    {
        _vehicleRepo = vehicleRepo;
        _employeeRepo = employeeRepo;
        _driverRepo = driverRepo;
        _vehicleAssignmentRepo = vehicleAssignmentRepo;
    }

    public async Task ValidateCreate(VehicleAssignmentAddDto dto)
    {
        var vehicle = await _vehicleRepo.FindByIdAsync(dto.VehicleId);
        if (vehicle == null || vehicle.RecStatus == RecStatusConstants.Deleted)
            throw new Exception("Vehicle not found");
        if (vehicle.VehicleStatus == VehicleStatus.Inactive)
            throw new Exception("Cannot assign to an inactive vehicle");

        var employee = await _employeeRepo.FindByIdAsync(dto.EmployeeId);
        if (employee == null || employee.RecStatus == RecStatusConstants.Deleted)
            throw new Exception("Employee not found");
        if (employee.EmployeeStatus != EmployeeStatus.Active)
            throw new Exception("Employee must be active to be assigned");

        if (employee.CurrentBranchId != vehicle.CurrentBranchId)
            throw new Exception("Employee must be from the same branch as the vehicle");

        if (dto.AssignmentType == VehicleAssignmentType.Driver)
        {
            if (employee.EmployeeType != EmployeeType.Driver)
                throw new Exception("Only employees with Driver type can be assigned as Driver");

            var hasDriver = await _driverRepo.CheckIfExistAsync(d =>
                d.EmployeeId == dto.EmployeeId && d.RecStatus != RecStatusConstants.Deleted);
            if (!hasDriver)
                throw new Exception("Employee must have an active Driver record to be assigned as Driver");

            var hasActiveDriver = await _vehicleAssignmentRepo.CheckIfExistAsync(a =>
                a.VehicleId == dto.VehicleId &&
                a.AssignmentType == VehicleAssignmentType.Driver &&
                a.IsActive &&
                a.RecStatus != RecStatusConstants.Deleted);
            if (hasActiveDriver)
                throw new Exception("Vehicle already has an active Driver assignment");
        }

        ValidateDates(dto.AssignedFrom, dto.AssignedTo);
        await ValidateOverlap(dto.VehicleId, dto.EmployeeId, dto.AssignmentType,
            dto.AssignedFrom, dto.AssignedTo, excludeId: null);
    }

    public async Task ValidateUpdate(VehicleAssignment assignment, VehicleAssignmentUpdateDto dto)
    {
        if (dto.AssignmentType == VehicleAssignmentType.Driver)
        {
            var employee = await _employeeRepo.FindByIdAsync(assignment.EmployeeId);
            if (employee.EmployeeType != EmployeeType.Driver)
                throw new Exception("Only employees with Driver type can be assigned as Driver");

            var hasDriver = await _driverRepo.CheckIfExistAsync(d =>
                d.EmployeeId == assignment.EmployeeId && d.RecStatus != RecStatusConstants.Deleted);
            if (!hasDriver)
                throw new Exception("Employee must have an active Driver record to be assigned as Driver");

            if (dto.IsActive)
            {
                var hasActiveDriver = await _vehicleAssignmentRepo.CheckIfExistAsync(a =>
                    a.VehicleId == assignment.VehicleId &&
                    a.AssignmentType == VehicleAssignmentType.Driver &&
                    a.IsActive &&
                    a.RecStatus != RecStatusConstants.Deleted &&
                    a.Id != assignment.Id);
                if (hasActiveDriver)
                    throw new Exception("Vehicle already has an active Driver assignment");
            }
        }

        ValidateDates(dto.AssignedFrom, dto.AssignedTo);
        await ValidateOverlap(assignment.VehicleId, assignment.EmployeeId, dto.AssignmentType,
            dto.AssignedFrom, dto.AssignedTo, excludeId: assignment.Id);
    }

    private void ValidateDates(DateTime assignedFrom, DateTime? assignedTo)
    {
        if (assignedTo.HasValue && assignedTo.Value < assignedFrom)
            throw new Exception("Assigned To date must be greater than or equal to Assigned From date");
    }

    private async Task ValidateOverlap(long vehicleId, long employeeId,
        VehicleAssignmentType assignmentType, DateTime assignedFrom, DateTime? assignedTo, long? excludeId)
    {
        var overlapping = await _vehicleAssignmentRepo.GetQueryable()
            .Where(a => a.VehicleId == vehicleId &&
                        a.EmployeeId == employeeId &&
                        a.AssignmentType == assignmentType &&
                        a.RecStatus != RecStatusConstants.Deleted &&
                        (excludeId == null || a.Id != excludeId))
            .Where(a => a.AssignedFrom < (assignedTo ?? DateTime.MaxValue) &&
                        (a.AssignedTo == null || a.AssignedTo > assignedFrom))
            .AnyAsync();

        if (overlapping)
            throw new Exception("Overlapping assignment exists for this Employee, Vehicle, and Assignment Type");
    }
}
