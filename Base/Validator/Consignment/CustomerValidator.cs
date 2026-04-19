using Base.Repo.Consignment.Interfaces;
using Base.Validator.Consignment.Interfaces;

namespace Base.Validator.Consignment;

public class CustomerValidator : ICustomerValidator
{
    private readonly ICustomerRepo _customerRepo;

    public CustomerValidator(ICustomerRepo customerRepo)
    {
        _customerRepo = customerRepo;
    }

    public async Task CheckDuplicatePhoneNo(string phoneNo, long? excludedId = null)
    {
        if (await _customerRepo.CheckIfExistAsync(a => a.PhoneNo == phoneNo && a.Id != excludedId))
            throw new Exception($"Duplicate Phone No: {phoneNo}");
    }

    public async Task CheckDuplicateEmail(string email, long? excludedId = null)
    {
        if (await _customerRepo.CheckIfExistAsync(a => a.Email == email && a.Id != excludedId))
            throw new Exception($"Duplicate Phone No: {email}");
    }
}