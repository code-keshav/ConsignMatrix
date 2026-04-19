using System.Transactions;
using Base.Dtos;
using Base.Entities;
using Base.Repo.Interfaces;
using Base.Services.Interfaces;
using Base.Validator.Interface;

namespace Base.Services;

public class VehicleAssignmentService : IVehicleAssignmentService
{
    private readonly IUow _uow;
    private readonly IVehicleAssignmentValidator _validator;

    public VehicleAssignmentService(IUow uow, IVehicleAssignmentValidator validator)
    {
        _uow = uow;
        _validator = validator;
    }

    public async Task CreateBulk(long vehicleId, List<VehicleAssignmentAddDto> dtos)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        foreach (var dto in dtos)
        {
            dto.VehicleId = vehicleId;
            await _validator.ValidateCreate(dto);

            var assignment = new VehicleAssignment
            {
                VehicleId = dto.VehicleId,
                EmployeeId = dto.EmployeeId,
                AssignmentType = dto.AssignmentType,
                AssignedFrom = dto.AssignedFrom,
                AssignedTo = dto.AssignedTo,
                IsActive = true
            };

            await _uow.CreateAsync(assignment);
        }

        await _uow.CommitAsync();
        scope.Complete();
    }

    public async Task Update(VehicleAssignment assignment, VehicleAssignmentUpdateDto dto)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        await _validator.ValidateUpdate(assignment, dto);

        assignment.AssignmentType = dto.AssignmentType;
        assignment.AssignedFrom = dto.AssignedFrom;
        assignment.AssignedTo = dto.AssignedTo;
        assignment.IsActive = dto.IsActive;

        _uow.Update(assignment);
        await _uow.CommitAsync();
        scope.Complete();
    }

    public async Task Delete(VehicleAssignment assignment)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        _uow.Remove(assignment);
        await _uow.CommitAsync();
        scope.Complete();
    }

    public async Task Deactivate(VehicleAssignment assignment)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        assignment.IsActive = false;
        _uow.Update(assignment);
        await _uow.CommitAsync();
        scope.Complete();
    }
}
