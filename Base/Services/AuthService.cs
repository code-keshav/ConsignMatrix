using System.Security.Claims;
using Base.Helpers;
using Base.Repo.Interfaces;
using Base.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace Base.Services;

public class AuthService : IAuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserRepo _userRepository;
    private readonly IUow _uow;

    public AuthService(IHttpContextAccessor httpContextAccessor, IUserRepo userRepository, IUow uow)
    {
        _httpContextAccessor = httpContextAccessor;
        _userRepository = userRepository;
        _uow = uow;
    }

    public async Task Login(string username, string password)
    {
        var user = await _userRepository.FindSingleOrThrowAsync(x => x.UserName.ToLower() == username.ToLower().Trim());
        if (user == null)
        {
            throw new Exception("User not found");
        }

        if (!Crypter.IsMatching(password, user.PasswordHash))
        {
            throw new Exception("Invalid password");
        }

        if (!user.IsActive)
        {
            throw new Exception("Your account has been deactivated. Please contact your administrator.");
        }

        var httpContext = _httpContextAccessor.HttpContext;
        var claims = new List<Claim>
        {
            new("Id", user.Id.ToString()),
            new("Name", user.UserName),
        };
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));

        user.LastLogin = DateTime.UtcNow;
        _uow.Update(user);
        await _uow.CommitAsync();
    }
    
    public async Task Logout()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
        // _extraClaimManager.RemoveExtraClaims();
        var identity = _httpContextAccessor.HttpContext?.User.Identity as ClaimsIdentity;
        var claims = identity.Claims.ToList();
        foreach (var claim in claims)
        {
            identity.RemoveClaim(claim);
        }
    }
}