using Base.Entities.Consignment;
using Base.Repo.Consignment.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Base.Repo.Consignment;

public class CustomerAddressRepo : GenericRepo<CustomerAddress>, ICustomerAddressRepo
{
    public CustomerAddressRepo(DbContext context) : base(context)
    {
    }

    public Task<List<CustomerAddress>> GetByCustomerId(long customerId) => GetQueryable().Where(c => c.CustomerId == customerId).ToListAsync();
}