using System.ComponentModel.DataAnnotations.Schema;

namespace Base.Entities.Consignment;

[Table("trip_consignment", Schema = "consignment")]
public class TripConsignment : BaseEntity
{
    public long TripId { get; set; }
    public virtual Trip Trip { get; set; }

    public long ConsignmentId { get; set; }
    public virtual Consignment Consignment { get; set; }

    public DateTime LoadedAt { get; set; }
}
