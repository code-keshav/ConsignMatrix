using System.Transactions;
using Base.Dtos.Consignment;
using Base.Entities.Consignment;
using Base.Repo.Consignment.Interfaces;
using Base.Repo.Interfaces;
using Base.Services.Consignment.Interfaces;

namespace Base.Services.Consignment;

public class CustomerAddressService : ICustomerAddressService
{
    private readonly IUow _uow;
    private readonly ICustomerAddressRepo _customerAddressRepo;

    public CustomerAddressService(IUow uow, ICustomerAddressRepo customerAddressRepo)
    {
        _uow = uow;
        _customerAddressRepo = customerAddressRepo;
    }

    public async Task<CustomerAddress> Create(CustomerAddressDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var customerAddress = new CustomerAddress
        {
            CustomerId = dto.CustomerId,
            AddressType = dto.AddressType,
            AddressLine1 = dto.AddressLine1,
            AddressLine2 = dto.AddressLine2,
            City = dto.City,
            State = dto.State,
            PinCode = dto.PinCode,
            Landmark = dto.Landmark,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            ContactNo = dto.ContactNo,
            IsDefault = dto.IsDefault
        };
        await _uow.CreateAsync(customerAddress);
        await _uow.CommitAsync();
        tx.Complete();
        return customerAddress;
    }

    public async Task Update(CustomerAddress address, CustomerAddressDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        address.AddressType = dto.AddressType;
        address.AddressLine1 = dto.AddressLine1;
        address.AddressLine2 = dto.AddressLine2;
        address.City = dto.City;
        address.State = dto.State;
        address.PinCode = dto.PinCode;
        address.Landmark = dto.Landmark;
        address.Latitude = dto.Latitude;
        address.Longitude = dto.Longitude;
        address.ContactNo = dto.ContactNo;
        _uow.Update(address);
        await _uow.CommitAsync();
        tx.Complete();
    }

    public async Task Delete(long customerId)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var customerAddresses = await _customerAddressRepo.GetByCustomerId(customerId);
        _uow.RemoveRange(customerAddresses);
        await _uow.CommitAsync();
        tx.Complete();
    }

    public async Task Activate(long customerId)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var customerAddresses = await _customerAddressRepo.GetByCustomerId(customerId);
        customerAddresses.ForEach(a => a.MarkAsActive());
        _uow.UpdateRange(customerAddresses);
        await _uow.CommitAsync();
        tx.Complete();
    }

    public async Task Deactivate(long customerId)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var customerAddresses = await _customerAddressRepo.GetByCustomerId(customerId);
        customerAddresses.ForEach(a => a.MarkAsInactive());
        _uow.UpdateRange(customerAddresses);
        await _uow.CommitAsync();
        tx.Complete();
    }
}