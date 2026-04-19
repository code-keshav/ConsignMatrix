using Base.Validator.Interface;

namespace Base.Validator;

public class DriverValidator : IDriverValidator
{
    public void ValidateLicenseExpiry(DateTime expiry)
    {
        if (expiry.Date < DateTime.UtcNow.Date)
            throw new Exception("License expiry date cannot be in the past");
    }
}
