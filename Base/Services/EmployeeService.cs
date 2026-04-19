using System.Transactions;
using Base.Dtos;
using Base.Entities;
using Base.Enum;
using Base.Repo.Interfaces;
using Base.Services.Interfaces;
using Base.Validator.Interface;

namespace Base.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IUow _uow;
    private readonly IEmployeeRepo _employeeRepo;
    private readonly IEmployeeValidator _employeeValidator;

    public EmployeeService(IUow uow, IEmployeeRepo employeeRepo, IEmployeeValidator employeeValidator)
    {
        _uow = uow;
        _employeeRepo = employeeRepo;
        _employeeValidator = employeeValidator;
    }

    public async Task<Employee> Create(EmployeeAddDto dto)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var employee = new Employee
        {
            EmployeeCode = dto.EmployeeCode,
            Name = dto.Name,
            Phone = dto.Phone,
            AlternatePhone = dto.AlternatePhone,
            Email = dto.Email,
            Address = dto.Address,
            EmployeeType = dto.EmployeeType,
            EmployeeStatus = dto.EmployeeStatus,
            JoiningDate = dto.JoiningDate,
            Department = dto.Department,
            Designation = dto.Designation,
            CurrentBranchId = dto.CurrentBranchId
        };
        _employeeValidator.ValidateEmployeeCode(employee, dto.EmployeeCode);
        await _uow.CreateAsync(employee);
        await _uow.CommitAsync();
        scope.Complete();
        return employee;
    }

    public async Task Update(Employee employee, EmployeeUpdateDto dto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        _employeeValidator.ValidateEmployeeCode(employee, dto.EmployeeCode);
        employee.EmployeeCode = dto.EmployeeCode;
        employee.Name = dto.Name;
        employee.Phone = dto.Phone;
        employee.AlternatePhone = dto.AlternatePhone;
        employee.Email = dto.Email;
        employee.Address = dto.Address;
        employee.EmployeeType = dto.EmployeeType;
        employee.EmployeeStatus = dto.EmployeeStatus;
        employee.JoiningDate = dto.JoiningDate;
        employee.TerminationDate = dto.TerminationDate;
        employee.Department = dto.Department;
        employee.Designation = dto.Designation;
        employee.CurrentBranchId = dto.CurrentBranchId;
        _uow.Update(employee);
        await _uow.CommitAsync();
        tx.Complete();
    }

    public async Task Delete(Employee employee)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        _uow.Remove(employee);
        await _uow.CommitAsync();
        tx.Complete();
    }
}
