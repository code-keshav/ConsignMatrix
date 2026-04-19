using Base.Entities;
using Base.Repo.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Base.Repo;

public class UserBranchTransferRepo : GenericRepo<UserBranchTransfer>, IUserBranchTransferRepo
{
    public UserBranchTransferRepo(DbContext context) : base(context)
    {
    }
}