using Base.Entities;
using Base.Repo.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Base.Repo;

public class EmployeeRepo : GenericRepo<Employee>, IEmployeeRepo
{
    public EmployeeRepo(DbContext context) : base(context)
    {
    }
}
