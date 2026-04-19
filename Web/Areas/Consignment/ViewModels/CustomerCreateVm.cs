using Base.Enum.Consignment;

namespace Web.Areas.Consignment.ViewModels;

public class CustomerCreateVm
{
    public required string Name { get; set; }
    public required string PhoneNo { get; set; }
    public string? SecondaryPhoneNo { get; set; }
    public string? Email { get; set; }
    public CustomerType CustomerType { get; set; }
    public CustomerAddressVm HomeAddress { get; set; }
}

public class CustomerVm : CustomerCreateVm
{
    public long Id { get; set; }
    public bool IsActive { get; set; }
}

