namespace Base.Validator.Consignment.Interfaces;

public interface ICustomerValidator
{
    Task CheckDuplicatePhoneNo(string phoneNo, long? excludedId = null);
    Task CheckDuplicateEmail(string email, long? excludedId = null);
}