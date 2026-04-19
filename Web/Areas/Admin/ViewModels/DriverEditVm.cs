namespace Web.Areas.Admin.ViewModels;

public class DriverEditVm
{
    public long Id { get; set; }
    public long EmployeeId { get; set; }

    // Read-only employee info
    public string EmployeeName { get; set; }
    public string EmployeeCode { get; set; }
    public string BranchName { get; set; }
    public string Phone { get; set; }

    // Editable fields
    public string LicenseNumber { get; set; }
    public DateTime LicenseExpiry { get; set; }
}
