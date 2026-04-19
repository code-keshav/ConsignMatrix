using Base.Enum.Consignment;

namespace Base.Dtos.Consignment;

public class CustomerDto
{
    public required string Name { get; set; }
    public required string PhoneNo { get; set; }
    public string? SecondaryPhoneNo { get; set; }
    public string? Email { get; set; }
    public CustomerType CustomerType { get; set; }
    public CustomerAddressDto AddressDto { get; set; }
}