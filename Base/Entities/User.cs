using System.ComponentModel.DataAnnotations.Schema;
using Base.Entities.Interfaces;
using Base.Enum;

namespace Base.Entities;

[Table("user", Schema = "Base")]
public class User : IBaseEntity
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string NormalizedUserName { get; set; }
    public string NormalizedEmail { get; set; }
    public string ContactNo { get; set; }
    public string? Address { get; set; }
    public string PasswordHash { get; set; }
    public string SecurityStamp { get; set; }
    public virtual Branch Branch { get; set; }
    public long BranchId { get; set; }

    public virtual List<UserRole> UserRoles { get; set; } = new();

    public List<Role> GetRoles()
    {
        return UserRoles.Select(x => x.Role).ToList();
    }

    public UserLevel UserLevel { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime? LastLogin { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedById { get; set; }
    public virtual User? CreatedByUser { get; set; }

    public bool IsSuperAdmin() => UserLevel == UserLevel.SuperAdmin;
    public bool IsAdmin() => UserLevel == UserLevel.Admin;
    public bool IsBranchAdmin() => UserLevel == UserLevel.BranchAdmin;
    public bool IsNormalUser() => UserLevel == UserLevel.User;
}