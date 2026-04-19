using System.Transactions;
using Base.Dtos.Consignment;
using Base.Entities.Consignment;
using Base.Repo.Consignment.Interfaces;
using Base.Repo.Interfaces;
using Base.Services.Consignment.Interfaces;

namespace Base.Services.Consignment;

public class CustomerService : ICustomerService
{
    private readonly IUow _uow;
    private readonly ICustomerAddressService _customerAddressService;
    private readonly IConsignmentRepo _consignmentRepo;

    public CustomerService(IUow uow, ICustomerAddressService customerAddressService,
        IConsignmentRepo consignmentRepo)
    {
        _uow = uow;
        _customerAddressService = customerAddressService;
        _consignmentRepo = consignmentRepo;
    }

    public async Task Create(CustomerDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var customer = await AddCustomer(dto);
        dto.AddressDto.CustomerId = customer.Id;
        await _customerAddressService.Create(dto.AddressDto);
        tx.Complete();
    }

    public async Task Update(Customer customer, CustomerDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        customer.Name = dto.Name;
        customer.PhoneNo = dto.PhoneNo;
        customer.SecondaryPhoneNo = dto.SecondaryPhoneNo;
        customer.Email = dto.Email;
        customer.CustomerType = dto.CustomerType;
        _uow.Update(customer);
        await _uow.CommitAsync();
        tx.Complete();
    }

    public async Task Delete(Customer customer)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        // Block deletion if customer is sender or receiver of any active consignment
        var hasActiveConsignments = await _consignmentRepo.FindByAsync(c =>
            c.SenderId == customer.Id || c.ReceiverId == customer.Id);
        if (hasActiveConsignments.Count > 0)
            throw new Exception("Cannot delete customer — they are associated with existing consignments.");

        _uow.Remove(customer);
        await _uow.CommitAsync();
        await _customerAddressService.Delete(customer.Id);
        tx.Complete();
    }

    public async Task Activate(Customer customer)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        customer.MarkAsActive();
        _uow.Update(customer);
        await _uow.CommitAsync();
        await _customerAddressService.Activate(customer.Id);
        tx.Complete();
    }

    public async Task Deactivate(Customer customer)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        customer.MarkAsInactive();
        _uow.Update(customer);
        await _uow.CommitAsync();
        await _customerAddressService.Deactivate(customer.Id);
        tx.Complete();
    }

    private async Task<Customer> AddCustomer(CustomerDto dto)
    {
        var customer = new Customer
        {
            Name = dto.Name,
            PhoneNo = dto.PhoneNo,
            SecondaryPhoneNo = dto.SecondaryPhoneNo,
            Email = dto.Email,
            CustomerType = dto.CustomerType,
        };
        await _uow.CreateAsync(customer);
        await _uow.CommitAsync();
        return customer;
    }
}