namespace Base.Validator.Interface;

public interface IDriverValidator
{
    void ValidateLicenseExpiry(DateTime expiry);
}
