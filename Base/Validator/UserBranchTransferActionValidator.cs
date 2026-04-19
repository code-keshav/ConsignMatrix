using Base.Entities;
using Base.Enum;
using Base.Repo.Interfaces;
using Base.Validator.Interface;

namespace Base.Validator;

public class UserBranchTransferActionValidator : IUserBranchTransferActionValidator
{
    private readonly IUserBranchTransferRepo _userBranchTransferRepo;

    public UserBranchTransferActionValidator(IUserBranchTransferRepo userBranchTransferRepo)
    {
        _userBranchTransferRepo = userBranchTransferRepo;
    }

    public async Task EnsureOnlyOnePendingRequest(User user)
    {
        if (await _userBranchTransferRepo.CheckIfExistAsync(x =>
                x.UserId == user.Id && x.TransferStatus == UserBranchTransferStatus.Requested))
        {
            throw new Exception($"Cannot perform action. Another transfer request for user `{user.Name}` is still pending. Please resolve that first");
        }
    }

    public void EnsureRequestIsForAnotherBranch(User user, Branch toBranch)
    {
        if (user.BranchId == toBranch.Id)
        {
            throw new Exception($"Cannot transfer user to branch {toBranch.Name} . `{user.Name}` is already part of the branch");
        }
    }

    public void EnsureResponseCanBeMade(UserBranchTransfer userBranchTransfer)
    {
        if (!userBranchTransfer.IsRequested())
        {
            throw new Exception($"Cannot respond to transfer request. The request has already been {userBranchTransfer.TransferStatus.ToString()}");
        }
    }
    
    public void EnsureNormalUserTransfer(User user)
    {
        if (!user.IsNormalUser())
        {
            throw new Exception($"Cannot transfer user. The user is an {user.UserLevel.ToString()}");
        }
    }
}