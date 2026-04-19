using Base.Enum.Consignment;

namespace Web.Areas.Consignment.ViewModels;

public class CustomerFilterVm
{
    public CustomerType? CustomerType { get; set; }
    public string? SearchTerm { get; set; }
    public List<CustomerVm> Customers { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}