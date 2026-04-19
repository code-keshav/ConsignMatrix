using Base.Entities;

namespace Base.Dtos;

public class UserBranchTransferResponseDto
{
    public User Responder { get; set; }
    public string InternalNote { get; set; }
}