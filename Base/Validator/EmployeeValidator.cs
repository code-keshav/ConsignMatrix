using Base.Entities;
using Base.Repo.Interfaces;
using Base.Validator.Interface;

namespace Base.Validator;

public class EmployeeValidator : IEmployeeValidator
{
    private readonly IEmployeeRepo _employeeRepo;

    public EmployeeValidator(IEmployeeRepo employeeRepo)
    {
        _employeeRepo = employeeRepo;
    }

    public void ValidateEmployeeCode(Employee employee, string code)
    {
        if (_employeeRepo.CheckIfExist(a => a.EmployeeCode == code && a.Id != employee.Id))
            throw new Exception($"Employee with code `{code}` already exists");
    }

    public void ValidateDriverFields(string? licenseNumber, DateTime? licenseExpiry)
    {
        if (string.IsNullOrWhiteSpace(licenseNumber))
            throw new Exception("License number is required for Driver type employees");
        if (!licenseExpiry.HasValue)
            throw new Exception("License expiry date is required for Driver type employees");
    }

    public void ValidateUserAccountFields(bool createUserAccount, string? email, string? password, string? confirmPassword)
    {
        if (!createUserAccount) return;
        if (string.IsNullOrWhiteSpace(email))
            throw new Exception("Email is required when creating a user account");
        if (string.IsNullOrWhiteSpace(password))
            throw new Exception("Password is required when creating a user account");
        if (password?.Trim().ToLower() != confirmPassword?.Trim().ToLower())
            throw new Exception("Password and confirm password do not match");
    }
}
