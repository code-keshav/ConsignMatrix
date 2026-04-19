using Base.Entities.Consignment;
using Base.Repo.Interfaces;

namespace Base.Repo.Consignment.Interfaces;

public interface ICustomerAddressRepo : IGenericRepo<CustomerAddress>
{
    Task<List<CustomerAddress>> GetByCustomerId(long customerId);
}