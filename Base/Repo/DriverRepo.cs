using Base.Entities;
using Base.Repo.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Base.Repo;

public class DriverRepo : GenericRepo<Driver>, IDriverRepo
{
    public DriverRepo(DbContext context) : base(context)
    {
    }
}
