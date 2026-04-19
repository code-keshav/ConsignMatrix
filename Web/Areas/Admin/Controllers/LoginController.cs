using Base.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Areas.Admin.Requests;
using Web.Helpers;

namespace Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("[area]/[controller]/[action]")]
[AllowAnonymous]
public class LoginController : Controller
{
    private readonly IAuthService _authService;
    private readonly INotificationHelper _notificationHelper;

    public LoginController(IAuthService authService, INotificationHelper notificationHelper)
    {
        _authService = authService;
        _notificationHelper = notificationHelper;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new LoginReq());
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginReq req)
    {
        try
        {
            await _authService.Login(req.Username, req.Password);
            _notificationHelper.SetSuccessMsg("User login successfully");
            return RedirectToAction(nameof(Index), "Home", new {Area = "Admin"});
        }
        catch (Exception e)
        {
            _notificationHelper.SetErrorMsg(e.Message);
            return RedirectToAction("Index");
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await _authService.Logout();
        return RedirectToAction("Index");
    }
}