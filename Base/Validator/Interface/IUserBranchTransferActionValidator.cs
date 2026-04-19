using Base.Entities;

namespace Base.Validator.Interface;

public interface IUserBranchTransferActionValidator
{
    Task EnsureOnlyOnePendingRequest(User user);
    void EnsureRequestIsForAnotherBranch(User user, Branch toBranch);
    void EnsureResponseCanBeMade(UserBranchTransfer userBranchTransfer);
    void EnsureNormalUserTransfer(User user);
}