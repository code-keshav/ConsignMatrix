using Base.Dtos;
using Base.Entities;

namespace Base.Services.Interfaces;

public interface IDriverService
{
    Task Update(Driver driver, DriverUpdateDto dto);
}
