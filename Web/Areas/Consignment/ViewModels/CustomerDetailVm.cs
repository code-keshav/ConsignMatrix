namespace Web.Areas.Consignment.ViewModels;

public class CustomerDetailVm : CustomerVm
{
    public string? Status { get; set; }
    public List<CustomerAddressVm> AddressVms { get; set; }
}