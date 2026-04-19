namespace Base.Dtos.Consignment;

public class CustomerAddressUpdateDto
{
    public long CustomerId { get; set; }
    List<CustomerAddressDto> CustomerAddressDtos { get; set; }
}