using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAPI.Infrastructure.Ui;
using WebAPI.Companies;

namespace WebAPI.Users;

public static class LoginUser
{
    public class Command
    {
        public string UserName { get; set; } = default!;
        public string Password { get; set; } = default!;
    }

    public static Task<RazorComponentResult> HandlePage()
    {
        return Task.FromResult<RazorComponentResult>(new RazorComponentResult<LoginPage>());
    }

    public static async Task<RazorComponentResult> HandleAction(HttpContext context, [FromBody] Command command, [FromServices] Company company)
    {
        if (command.UserName != company.User || command.Password != company.Password)
        {
            return new RazorComponentResult<Alert>(new { Text = "Invalid username or password. Please try again" });
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, command.UserName),
            new Claim(ClaimTypes.Role, "Administrator"),
        };

        var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            AllowRefresh = true
        };
        await context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        context.Response.Headers.Append("HX-Redirect", "/ui");

        return new RazorComponentResult<Alert>(new { Text = "" });
    }
}
