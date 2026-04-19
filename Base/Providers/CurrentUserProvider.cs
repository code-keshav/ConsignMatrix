using Base.Constants;
using Base.Entities;
using Base.Providers.Interfaces;
using Base.Repo.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Base.Providers;

public class CurrentUserProvider : ICurrentUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserRepo _userRepo;
    private readonly IBranchRepo _branchRepo;
    private User? user = null;
    private Branch? branch = null;

    public CurrentUserProvider(IHttpContextAccessor httpContextAccessor, IUserRepo userRepo, IBranchRepo branchRepo)
    {
        _httpContextAccessor = httpContextAccessor;
        _userRepo = userRepo;
        _branchRepo = branchRepo;
    }

    public long GetUserId()
    {
        var id = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id");
        if (id == null) throw new Exception("User Id not found");
        return Convert.ToInt64(id.Value);
    }

    public async Task<User> GetCurrentUser()
    {
        var userId = GetUserId();
        return user ??= await _userRepo.GetQueryable().Include(x => x.Branch).FirstOrDefaultAsync(x => x.Id == userId) ?? throw new Exception("User Not Found");
    }

    public async Task<long> GetUserBranchId()
    {
        var branch = await GetUserBranch();
        return branch.Id;
    }
    
    public async Task<Branch> GetUserBranch()
    {
        if (branch == null)
        {
            var cu = await GetCurrentUser();
            branch = await _branchRepo.FindOrThrowAsync(cu.BranchId);
        }
        return branch;
    }

    public async Task ValidateBranchUsage(long branchId)
    {
        if (await GetUserBranchId() != branchId)
        {
            throw new Exception("Error: Access Denied");
        }
    }

    public async Task ValidateBranchUsage(IEnumerable<long> branchIds)
    {
        var currentUserBranchId = await GetUserBranchId();
        if (branchIds.Any(a => a != currentUserBranchId))
        {
            throw new Exception("Error: Access Denied");
        }
    }

    public async Task<bool> IsMainBranch() => await GetUserBranchId() == (long)IdConstants.MainBranchId;
}