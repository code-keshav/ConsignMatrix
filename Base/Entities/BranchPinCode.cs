using System.ComponentModel.DataAnnotations.Schema;

namespace Base.Entities;

[Table("branch_pin_code", Schema = "Base")]
public class BranchPinCode : BaseEntity
{
    public long BranchId { get; set; }
    public string PinCode { get; set; }
    public bool IsActive { get; set; } = true;
    public virtual Branch Branch { get; set; }
}
