using Base.Entities;
using Base.Repo.Interfaces;
using Base.Validator.Interface;

namespace Base.Validator;

public class UserValidator : IUserValidator
{
    private readonly IUserRepo _userRepo;

    public UserValidator(IUserRepo userRepo)
    {
        _userRepo = userRepo;
    }

    public void ValidatePassword(string password)
    {
        if (password.Length < 5) throw new Exception("Password must be at least 5 character long");
        if (!password.Any(char.IsDigit)) throw new Exception("Password should contain at least one number");
    }

    public void ValidateUserEmail(User user, string email)
    {
        if (_userRepo.CheckIfExist(a => a.Id != user.Id && a.Email.Trim() == email.Trim()))
            throw new Exception($"User with email `{email}` already exists");
    }
}