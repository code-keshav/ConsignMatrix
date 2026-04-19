using Base.Enum.Consignment;

namespace Base.Dtos.Consignment;

public class CustomerAddressDto
{
    public long CustomerId { get; set; }
    public AddressType AddressType { get; set; }
    public required string AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public required string PinCode { get; set; }
    public string? Landmark { get; set; }
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }
    public string? ContactNo { get; set; }
    public bool IsDefault { get; set; }
}