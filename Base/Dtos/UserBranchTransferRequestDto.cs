using Base.Entities;

namespace Base.Dtos;

public class UserBranchTransferRequestDto
{
    public UserBranchTransferRequestDto(User user, User initiator, string requestNote, Branch toBranch)
    {
        User = user;
        Initiator = initiator;
        RequestNote = requestNote;
        ToBranch = toBranch;
    }

    public User User { get; set; }
    public Branch ToBranch { get; set; }
    public string RequestNote { get; set; }
    public User Initiator { get; set; }
}