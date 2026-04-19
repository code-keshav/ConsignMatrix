using Base.Dtos.Consignment;
using Base.Entities.Consignment;

namespace Base.Services.Consignment.Interfaces;

public interface ICustomerService
{
    Task Create(CustomerDto dto);
    Task Update(Customer customer, CustomerDto dto);
    Task Delete(Customer customer);
    Task Activate(Customer customer);
    Task Deactivate(Customer customer);
}