using Base.Dtos;
using Base.Entities;

namespace Base.Manager.Interface;

public interface IBranchManager
{
    Task Create(BranchDto dto);
}