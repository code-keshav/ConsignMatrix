using System.Text;
using Acl.Helper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using Web.Configuration;
using Web.Extensions;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
ExcelPackage.License.SetNonCommercialOrganization("CoffeeCoders");

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Add services
services.AddOpenApi();
services.AddControllers(config =>
{
    var policy = new AuthorizationPolicyBuilder("smart")
        .RequireAuthenticatedUser()
        .Build();
    config.Filters.Add(new AuthorizeFilter(policy));
});
services.AddControllersWithViews().AddNewtonsoftJson().AddRazorRuntimeCompilation();
services.AddHttpContextAccessor();
services.ConfigureAppDi();

services.AddAuthentication(opt =>
    {
        opt.DefaultAuthenticateScheme = "smart";
        opt.DefaultChallengeScheme = "smart";
    })
    .AddPolicyScheme("smart", "JWT or Identity Cookie", options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (authHeader?.ToLower().StartsWith("bearer ") == true)
            {
                return JwtBearerDefaults.AuthenticationScheme;
            }

            return CookieAuthenticationDefaults.AuthenticationScheme;
        };
    })
    .AddCookie(cfg =>
    {
        cfg.SlidingExpiration = true;
        cfg.ExpireTimeSpan = TimeSpan.FromDays(2);
        cfg.LoginPath = "/admin/login/index";
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetJwtKey())),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/admin/login/index";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.Name = "auth";
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
        else
        {
            context.Response.Redirect("/admin/login/index");
        }

        return Task.CompletedTask;
    };
});

services.AddAuthorization(options =>
{
    var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder("smart")
        .RequireAuthenticatedUser();
    options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
});

var app = builder.Build();

// Run migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
ConfigureStaticFiles(app);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute("areas", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}", defaults: new { area = "Admin" });

app.Run();

void ConfigureStaticFiles(WebApplication webApplication)
{
    var provider = new FileExtensionContentTypeProvider();
    provider.Mappings[".js"] = "application/javascript";

    webApplication.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(
            Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
        ContentTypeProvider = provider
    });

    var contentDir = app.Configuration["contentDir"];
    if (!string.IsNullOrEmpty(contentDir))
    {
        webApplication.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(contentDir!),
            RequestPath = "/uploads"
        });
    }
}
