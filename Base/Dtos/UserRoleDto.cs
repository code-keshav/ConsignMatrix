using Base.Entities;

namespace Base.Dtos;

public class UserRoleDto
{
    public User User { get; set; }
    public List<Role> Roles { get; set; }
    public Branch Branch { get; set; }
}