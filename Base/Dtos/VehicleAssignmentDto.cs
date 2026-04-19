using Base.Enum;

namespace Base.Dtos;

public class VehicleAssignmentAddDto
{
    public long VehicleId { get; set; }
    public long EmployeeId { get; set; }
    public VehicleAssignmentType AssignmentType { get; set; }
    public DateTime AssignedFrom { get; set; }
    public DateTime? AssignedTo { get; set; }
}

public class VehicleAssignmentUpdateDto
{
    public VehicleAssignmentType AssignmentType { get; set; }
    public DateTime AssignedFrom { get; set; }
    public DateTime? AssignedTo { get; set; }
    public bool IsActive { get; set; }
}
