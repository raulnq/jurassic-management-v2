namespace WebAPI.Clients;

public static class Endpoints
{
    public const string Title = "Clients";

    public const string List = "/ui/clients/list";

    public const string ListTitle = "List clients";

    public const string Register = "/ui/clients/register";

    public const string RegisterTitle = "Register clients";

    public const string Edit = "/ui/clients/{clientId}/edit";

    public const string EditTitle = "Edit client";

    public static void RegisterClientEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/clients")
        .WithTags("clients");

        group.MapPost("/", RegisterClient.Handle);

        group.MapGet("/", ListClients.Handle);

        group.MapGet("/{clientId:guid}", GetClient.Handle);

        group.MapPut("/{clientId:guid}", EditClient.Handle);

        var uigroup = app.MapGroup("/ui/clients")
        .ExcludeFromDescription()
        .RequireAuthorization();

        uigroup.MapGet("/list", ListClients.HandlePage);

        uigroup.MapGet("/register", RegisterClient.HandlePage);

        uigroup.MapPost("/register", RegisterClient.HandleAction);

        uigroup.MapGet("/{clientId:guid}/edit", EditClient.HandlePage);

        uigroup.MapPost("/{clientId:guid}/edit", EditClient.HandleAction);
    }
}