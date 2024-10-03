namespace WebAPI.CollaboratorRoles;

public static class Endpoints
{
    public const string Title = "Collaborator Roles";

    public const string ListTitle = "List collaborator roles";

    public const string List = "/ui/collaborator-roles/list";

    public const string RegisterTitle = "Register collaborator role";

    public const string Register = "/ui/collaborator-roles/register";

    public const string EditTitle = "Edit collaborator role";

    public const string Edit = "/ui/collaborator-roles/{collaboratorRoleId}/edit";

    public static void RegisterCollaboratorRoleEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/collaborator-roles")
        .WithTags("collaborator-roles");

        group.MapPost("/", RegisterCollaboratorRole.Handle);

        group.MapGet("/", ListCollaboratorRoles.Handle);

        group.MapGet("/{collaboratorRoleId:guid}", GetCollaboratorRole.Handle);

        group.MapPut("/{collaboratorRoleId:guid}", EditCollaboratorRole.Handle);

        var uigroup = app.MapGroup("/ui/collaborator-roles")
        .ExcludeFromDescription()
        .RequireAuthorization();

        uigroup.MapGet("/list", ListCollaboratorRoles.HandlePage);

        uigroup.MapGet("/register", RegisterCollaboratorRole.HandlePage);

        uigroup.MapPost("/register", RegisterCollaboratorRole.HandleAction);

        uigroup.MapGet("/{collaboratorRoleId:guid}/edit", EditCollaboratorRole.HandlePage);

        uigroup.MapPost("/{collaboratorRoleId:guid}/edit", EditCollaboratorRole.HandleAction);
    }
}
