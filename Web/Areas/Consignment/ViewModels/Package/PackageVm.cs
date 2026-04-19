using Base.Enum.Consignment;

namespace Web.Areas.Consignment.ViewModels.Package;

public class PackageVm
{
    public long Id { get; set; }
    public long ConsignmentId { get; set; }
    public int PackageNumber { get; set; }
    public string Barcode { get; set; }
    public decimal Weight { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public decimal Volume { get; set; }
    public decimal VolumetricWeight { get; set; }
    public PackageType PackageType { get; set; }
    public string? ContentDescription { get; set; }
    public bool IsFragile { get; set; }
    public bool IsHazardous { get; set; }
    public bool CanBeStacked { get; set; }
    public bool RequiresColdChain { get; set; }
}
