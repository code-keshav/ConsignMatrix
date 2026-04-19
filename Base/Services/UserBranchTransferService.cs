using System.Transactions;
using Base.Dtos;
using Base.Entities;
using Base.Repo.Interfaces;
using Base.Services.Interfaces;
using Base.Validator.Interface;

namespace Base.Services;

public class UserBranchTransferService : IUserBranchTransferService
{
    private readonly IBranchRepo _branchRepo;
    private readonly IUow _uow;
    private readonly IUserBranchTransferActionValidator _userBranchTransferActionValidator;

    public UserBranchTransferService(IBranchRepo branchRepo, IUow uow, IUserBranchTransferActionValidator userBranchTransferActionValidator)
    {
        _branchRepo = branchRepo;
        _uow = uow;
        _userBranchTransferActionValidator = userBranchTransferActionValidator;
    }

    public async Task<UserBranchTransfer> InitiateRequest(UserBranchTransferRequestDto requestDto)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var fromBranch = await _branchRepo.FindOrThrowAsync(requestDto.User.BranchId);
        await _userBranchTransferActionValidator.EnsureOnlyOnePendingRequest(requestDto.User);
        _userBranchTransferActionValidator.EnsureRequestIsForAnotherBranch(requestDto.User,
            requestDto.ToBranch);

        var branchTransfer = new UserBranchTransfer(requestDto.User, fromBranch, requestDto.ToBranch,
            requestDto.RequestNote, requestDto.Initiator);
        await _uow.CreateAsync(branchTransfer);
        await _uow.CommitAsync();
        tx.Complete();
        return branchTransfer;
    }

    public async Task ApproveRequest(UserBranchTransfer userBranchTransfer, UserBranchTransferResponseDto dto,
        bool forceApprove = false)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        _userBranchTransferActionValidator.EnsureResponseCanBeMade(userBranchTransfer);
        if (!forceApprove)
        {
            // The user status could have changed after the request was made.
            _userBranchTransferActionValidator.EnsureNormalUserTransfer(userBranchTransfer.User);
        }

        userBranchTransfer.Approve(dto.Responder, dto.InternalNote);
        userBranchTransfer.User.BranchId = userBranchTransfer.ToBranchId;
        _uow.Update(userBranchTransfer);
        await _uow.CommitAsync();
        tx.Complete();
    }
}