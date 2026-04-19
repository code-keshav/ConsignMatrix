using Base.Entities;

namespace Base.Repo.Interfaces;

public interface IRoleRepo : IGenericRepo<Role>
{
    Task<List<Role>> GetRoles();
}