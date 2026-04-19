using System.ComponentModel.DataAnnotations.Schema;

namespace Base.Entities;

[Table("role", Schema = "acl")]
public class Role : BaseEntity
{
    protected Role()
    {
    }

    public Role(string name, long? priority, Branch branch, bool isGlobal)
    {
        Name = name;
        Priority = priority;
        Branch = branch;
        IsGlobal = isGlobal;
    }

    public string Name { get; set; }
    public long? Priority { get; set; }
    public virtual Branch Branch { get; set; }
    public long BranchId { get; set; }
    public StatusEnum Status { get; set; } = StatusEnum.Active;
    public bool IsGlobal { get; set; }

    public void ToggleGlobalRole(bool value, Branch branch)
    {
        IsGlobal = value;
        Branch = branch;
    }
}