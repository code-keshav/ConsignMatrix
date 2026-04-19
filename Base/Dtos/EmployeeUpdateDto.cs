using Base.Enum;

namespace Base.Dtos;

public class EmployeeUpdateDto
{
    public string EmployeeCode { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public string? AlternatePhone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public EmployeeType EmployeeType { get; set; }
    public EmployeeStatus EmployeeStatus { get; set; }
    public DateTime JoiningDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public string? Department { get; set; }
    public string? Designation { get; set; }
    public long CurrentBranchId { get; set; }

    // Driver fields (when EmployeeType == Driver)
    public string? LicenseNumber { get; set; }
    public DateTime? LicenseExpiry { get; set; }
}
