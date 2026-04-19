using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Base.Extensions;

public static class HttpContextExtension
{
    public static string GetUrlPermission(this HttpContext context)
    {
        var data = context.GetRouteData().Values;
        var requirement = "/";
        if (data.ContainsKey("area"))
        {
            requirement += data["area"].ToString().ToLower() + "/";
        }
        requirement += data["controller"] + "/" + data["action"];
        return requirement;
    }
}