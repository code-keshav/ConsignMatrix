using Base.Entities;
using Base.Repo.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Base.Repo;

public class VehicleRepo : GenericRepo<Vehicle>, IVehicleRepo
{
    public VehicleRepo(DbContext context) : base(context)
    {
    }
}
