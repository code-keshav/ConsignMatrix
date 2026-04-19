using Base.Dtos;

namespace Base.Manager.Interface;

public interface IEmployeeManager
{
    Task Create(EmployeeAddDto dto);
    Task Update(long id, EmployeeUpdateDto dto);
    Task Delete(long id);
}
