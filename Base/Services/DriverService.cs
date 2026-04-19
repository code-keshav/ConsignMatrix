using System.Transactions;
using Base.Dtos;
using Base.Entities;
using Base.Repo.Interfaces;
using Base.Services.Interfaces;
using Base.Validator.Interface;

namespace Base.Services;

public class DriverService : IDriverService
{
    private readonly IUow _uow;
    private readonly IDriverValidator _driverValidator;

    public DriverService(IUow uow, IDriverValidator driverValidator)
    {
        _uow = uow;
        _driverValidator = driverValidator;
    }

    public async Task Update(Driver driver, DriverUpdateDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        _driverValidator.ValidateLicenseExpiry(dto.LicenseExpiry);
        driver.LicenseNumber = dto.LicenseNumber;
        driver.LicenseExpiry = dto.LicenseExpiry;
        _uow.Update(driver);
        await _uow.CommitAsync();
        tx.Complete();
    }
}
