using Base.Entities;

namespace Base.Validator.Interface;

public interface IEmployeeValidator
{
    void ValidateEmployeeCode(Employee employee, string code);
    void ValidateDriverFields(string? licenseNumber, DateTime? licenseExpiry);
    void ValidateUserAccountFields(bool createUserAccount, string? email, string? password, string? confirmPassword);
}
