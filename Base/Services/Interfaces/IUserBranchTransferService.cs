using Base.Dtos;
using Base.Entities;

namespace Base.Services.Interfaces;

public interface IUserBranchTransferService
{
    Task<UserBranchTransfer> InitiateRequest(UserBranchTransferRequestDto requestDto);

    Task ApproveRequest(UserBranchTransfer userBranchTransfer, UserBranchTransferResponseDto dto,
        bool forceApprove = false);
}