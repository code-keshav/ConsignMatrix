using Acl.Helper.Interface;
using Base.Constants;
using Base.Providers.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Web.Attribute;

public class MainBranchOnlyAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var currentUserProvider = context.HttpContext.RequestServices
            .GetRequiredService<ICurrentUserProvider>();
        var permission = context.HttpContext.RequestServices.GetRequiredService<IPermissionProvider>();
        var branch = currentUserProvider.GetUserBranch().Result;
        if (branch == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }
        if (branch.Id != (long)IdConstants.MainBranchId)
        {
            context.Result = new ForbidResult(); // Or throw a specific exception
        }
        base.OnActionExecuting(context);
    }
}