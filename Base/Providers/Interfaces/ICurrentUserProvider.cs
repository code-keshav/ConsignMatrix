using Base.Entities;

namespace Base.Providers.Interfaces;

public interface ICurrentUserProvider
{
    long GetUserId();
    Task<User> GetCurrentUser();
    Task<long> GetUserBranchId();
    Task<Branch> GetUserBranch();

    Task ValidateBranchUsage(long branchId);
    Task ValidateBranchUsage(IEnumerable<long> branchIds);
    Task<bool> IsMainBranch();
}