using Base.Entities;

namespace Base.Validator.Interface;

public interface IUserValidator
{
    void ValidatePassword(string password);
    void ValidateUserEmail(User user, string email);
}