using Base.Entities;
using Base.Repo.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Base.Repo;

public class UserRoleRepo : GenericRepo<UserRole>, IUserRoleRepo
{
    public UserRoleRepo(DbContext context) : base(context)
    {
    }
}