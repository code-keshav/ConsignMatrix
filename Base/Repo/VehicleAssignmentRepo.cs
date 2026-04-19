using Base.Entities;
using Base.Repo.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Base.Repo;

public class VehicleAssignmentRepo : GenericRepo<VehicleAssignment>, IVehicleAssignmentRepo
{
    public VehicleAssignmentRepo(DbContext context) : base(context)
    {
    }
}
