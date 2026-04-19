using System.ComponentModel.DataAnnotations.Schema;
using Base.Entities.Interfaces;

namespace Base.Entities;

[Table("driver", Schema = "Base")]
public class Driver : BaseEntity, ISoftDelete
{
    public string LicenseNumber { get; set; }
    public DateTime LicenseExpiry { get; set; }

    public virtual Employee Employee { get; set; }
    public long EmployeeId { get; set; }
}
