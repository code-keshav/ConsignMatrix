using System.ComponentModel.DataAnnotations.Schema;
using Base.Entities.Interfaces;
using Base.Enum;

namespace Base.Entities;

[Table("employee", Schema = "Base")]
public class Employee : BaseEntity, ISoftDelete
{
    public string EmployeeCode { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public string? AlternatePhone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public EmployeeType EmployeeType { get; set; }
    public EmployeeStatus EmployeeStatus { get; set; } = EmployeeStatus.Active;
    public DateTime JoiningDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public string? Department { get; set; }
    public string? Designation { get; set; }

    public virtual Branch CurrentBranch { get; set; }
    public long CurrentBranchId { get; set; }

    public virtual User? User { get; set; }
    public long? UserId { get; set; }

    public virtual Driver? Driver { get; set; }
}
