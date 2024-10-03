namespace WebAPI.CollaboratorBalance;

public static class Endpoints
{
    public const string Title = "Collaborator Balance";

    public const string ListTitle = "List collaborator balance";

    public const string List = "/ui/collaborator-balance/list";

    public static void RegisterCollaboratorBalanceEndpoints(this WebApplication app)
    {
        var uigroup = app.MapGroup("/ui/collaborator-balance")
        .ExcludeFromDescription()
        .RequireAuthorization();

        uigroup.MapGet("/list", ListCollaboratorBalance.HandlePage);
    }
}
