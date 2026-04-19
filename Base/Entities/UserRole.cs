using System.ComponentModel.DataAnnotations.Schema;

namespace Base.Entities;

[Table("user_role", Schema = "acl")]
public class UserRole : BaseEntity
{
    protected UserRole()
    {
    }

    public UserRole(User user, Role role, Branch branch)
    {
        User = user;
        Role = role;
        Branch = branch;
    }

    public virtual User User { get; protected set; }
    public long UserId { get; protected set; }

    public virtual Role Role { get; protected set; }
    public long RoleId { get; protected set; }

    public virtual Branch Branch { get; protected set; }
    public long BranchId { get; protected set; }
    public void UpdateRole(Role role)
    {
        Role = role;
    }
}