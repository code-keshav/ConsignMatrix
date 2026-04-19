using Base.Enum.Consignment;

namespace Web.Areas.Consignment.ViewModels;

public class CustomerAddressVm
{
    public long CustomerId { get; set; }
    public long AddressId { get; set; }
    public AddressType AddressType { get; set; }
    public string AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string PinCode { get; set; }
    public string? Landmark { get; set; }
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }
    public string? ContactNo { get; set; }
    public bool IsDefault { get; set; }
    public string? CustomerName { get; set; }
}