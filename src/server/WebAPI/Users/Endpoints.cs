using Microsoft.AspNetCore.Http.HttpResults;

namespace WebAPI.Users;

public static class Endpoints
{
    public const string Login = "/";

    public static void RegisterUserEndpoints(this WebApplication app)
    {
        app.MapGet("/", LoginUser.HandlePage).ExcludeFromDescription();

        app.MapPost("/", LoginUser.HandleAction).ExcludeFromDescription();

        var uigroup = app.MapGroup("/ui")
        .ExcludeFromDescription()
        .RequireAuthorization();

        uigroup.MapGet("/", () =>
        {
            return new RazorComponentResult<MainPage>();
        });

    }
}
