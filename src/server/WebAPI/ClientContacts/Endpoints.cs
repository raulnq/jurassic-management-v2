namespace WebAPI.ClientContacts;

public static class Endpoints
{
    public const string Title = "Contacts";

    public const string List = "/ui/clients/{clientId}/contacts/list";

    public const string Add = "/ui/clients/{clientId}/contacts/add";

    public const string AddTitle = "Add contact";

    public const string Edit = "/ui/clients/{clientId}/contacts/{clientContactId}/edit";

    public const string EditTitle = "Edit contact";

    public static void RegisterClientContactEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/clients")
        .WithTags("clients");

        group.MapPost("/{clientId:guid}/contacts", AddClientContact.Handle);

        group.MapGet("/{clientId:guid}/contacts", ListClientContacts.Handle);

        group.MapPut("/{clientId:guid}/contacts/{clientContactId:guid}", EditClientContact.Handle);

        var uigroup = app.MapGroup("/ui/clients")
        .ExcludeFromDescription();

        uigroup.MapGet("/{clientId:guid}/contacts/list", ListClientContacts.HandlePage);

        uigroup.MapGet("/{clientId:guid}/contacts/add", AddClientContact.HandlePage);

        uigroup.MapPost("/{clientId:guid}/contacts/add", AddClientContact.HandleAction);

        uigroup.MapGet("/{clientId:guid}/contacts/{clientContactId:guid}/edit", EditClientContact.HandlePage);

        uigroup.MapPost("/{clientId:guid}/contacts/{clientContactId:guid}/edit", EditClientContact.HandleAction);
    }
}