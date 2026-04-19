using System.ComponentModel.DataAnnotations.Schema;
using Base.Entities.Interfaces;
using Base.Enum.Consignment;

namespace Base.Entities.Consignment;

[Table("customer", Schema = "consignment")]
public class Customer : BaseEntity, ISoftDelete
{
    public required string Name { get; set; }
    public required string PhoneNo { get; set; }
    public string? SecondaryPhoneNo { get; set; }
    public string? Email { get; set; }
    public CustomerType CustomerType { get; set; }
    public virtual List<CustomerAddress> CustomerAddresses { get; set; }
}