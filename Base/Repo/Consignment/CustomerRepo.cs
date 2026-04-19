using Base.Entities.Consignment;
using Base.Repo.Consignment.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Base.Repo.Consignment;

public class CustomerRepo : GenericRepo<Customer>, ICustomerRepo
{
    public CustomerRepo(DbContext context) : base(context)
    {
    }
    
}