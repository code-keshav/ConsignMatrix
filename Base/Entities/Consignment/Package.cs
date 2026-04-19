using System.ComponentModel.DataAnnotations.Schema;
using Base.Entities.Interfaces;
using Base.Enum.Consignment;

namespace Base.Entities.Consignment;

[Table("package", Schema = "consignment")]
public class Package : BaseEntity, ISoftDelete
{
    public long ConsignmentId { get; set; }
    public virtual Consignment Consignment { get; set; }

    public int PackageNumber { get; set; }
    public required string Barcode { get; set; }

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
    public bool CanBeStacked { get; set; } = true;
    public bool RequiresColdChain { get; set; }
}
