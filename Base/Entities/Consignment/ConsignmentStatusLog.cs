using System.ComponentModel.DataAnnotations.Schema;
using Base.Enum.Consignment;

namespace Base.Entities.Consignment;

[Table("consignment_status_log", Schema = "consignment")]
public class ConsignmentStatusLog : BaseEntity
{
    public long ConsignmentId { get; set; }
    public virtual Consignment Consignment { get; set; }
    public ConsignmentStatusType StatusType { get; set; }
    public string? Remarks { get; set; }
}
