using Base.Dtos;
using Base.Entities;

namespace Base.Services.Interfaces;

public interface IBranchPinCodeService
{
    Task<BranchPinCode> Create(BranchPinCodeDto dto);
    Task Update(BranchPinCode pinCode, BranchPinCodeDto dto);
    Task Delete(BranchPinCode pinCode);
    Task<List<Branch>> CheckServiceability(string pinCode);
}
