using Base.Dtos.Consignment;
using ConsignmentEntity = Base.Entities.Consignment.Consignment;

namespace Base.Services.Consignment.Interfaces;

public interface IConsignmentService
{
    Task Create(ConsignmentDto dto);
    Task Update(ConsignmentEntity consignment, ConsignmentUpdateDto dto);
    Task Delete(ConsignmentEntity consignment);
    Task Activate(ConsignmentEntity consignment);
    Task Deactivate(ConsignmentEntity consignment);
}
