using Base.Dtos;
using Base.Entities;

namespace Base.Services.Interfaces;

public interface IBranchService
{
    Task<Branch> Create(BranchDto dto);
    Task Update(Branch branch, BranchDto dto);
    Task Activate(Branch branch);
    Task Deactivate(Branch branch);
    Task<BranchImportResultDto> ImportFromExcel(List<BranchImportDto> branches);
    Task Delete(Branch branch);
}