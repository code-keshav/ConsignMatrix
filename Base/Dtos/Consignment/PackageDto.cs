using Base.Enum.Consignment;

namespace Base.Dtos.Consignment;

public class PackageDto
{
    public decimal Weight { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public PackageType PackageType { get; set; }
    public string? ContentDescription { get; set; }
    public bool IsFragile { get; set; }
    public bool IsHazardous { get; set; }
    public bool CanBeStacked { get; set; } = true;
    public bool RequiresColdChain { get; set; }
}
