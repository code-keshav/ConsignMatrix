using Base.Dtos.Consignment;
using Base.Entities.Consignment;

namespace Base.Services.Consignment.Interfaces;

public interface ICustomerAddressService
{
    Task<CustomerAddress> Create(CustomerAddressDto dto);
    Task Update(CustomerAddress address, CustomerAddressDto dto);
    Task Delete(long customerId);
    Task Activate(long customerId);
    Task Deactivate(long customerId);
}