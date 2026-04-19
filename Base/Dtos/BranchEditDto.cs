using Base.Enum;

namespace Base.Dtos;

public class BranchEditDto
{
    public string Name { get; set; }
    public long Code { get; set; }
    public BranchType BranchType { get; set; } = BranchType.ServiceCenter;
    public string Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string ContactNo { get; set; }
    public string? Email { get; set; }
    public decimal? StorageCapacity { get; set; }
    public string? OperatingHours { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}
