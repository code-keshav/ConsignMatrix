using System.ComponentModel.DataAnnotations.Schema;
using Base.Entities.Interfaces;
using Base.Enum;

namespace Base.Entities;

[Table("branch", Schema = "Base")]
public class Branch : IBaseEntity
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public BranchType BranchType { get; set; } = BranchType.ServiceCenter;
    public string Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string ContactNo { get; set; }
    public string? Email { get; set; }
    public decimal? StorageCapacity { get; set; }
    public decimal CurrentLoad { get; set; }
    public string? OperatingHours { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public StatusEnum Status { get; set; } = StatusEnum.Active;
    public virtual ICollection<BranchPinCode> PinCodes { get; set; } = new List<BranchPinCode>();

    public BranchMiniInfo ToMiniInfo => new()
    {
        Id = Id,
        Name = Name,
        Code = Code,
        Address = Address,
        ContactNo = ContactNo,
        Email = Email,
        Status = Status
    };
}

public class BranchMiniInfo
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string Address { get; set; }
    public string ContactNo { get; set; }
    public string? Email { get; set; }
    public StatusEnum Status { get; set; } = StatusEnum.Active;
}
