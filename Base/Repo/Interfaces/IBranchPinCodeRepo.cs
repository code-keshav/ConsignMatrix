using Base.Entities;

namespace Base.Repo.Interfaces;

public interface IBranchPinCodeRepo : IGenericRepo<BranchPinCode>
{
    Task<List<BranchPinCode>> GetByBranchId(long branchId);
    Task<List<Branch>> GetBranchesServingPinCode(string pinCode);
}
