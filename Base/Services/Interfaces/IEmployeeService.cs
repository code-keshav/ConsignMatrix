using Base.Dtos;
using Base.Entities;

namespace Base.Services.Interfaces;

public interface IEmployeeService
{
    Task<Employee> Create(EmployeeAddDto dto);
    Task Update(Employee employee, EmployeeUpdateDto dto);
    Task Delete(Employee employee);
}
