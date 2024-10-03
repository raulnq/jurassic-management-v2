namespace WebAPI.Collections;

public static class Endpoints
{
    public const string Title = "Collections";

    public const string List = "/ui/collections/list";

    public const string ListTitle = "List collections";

    public const string View = "/ui/collections/{collectionId}/view";

    public const string ViewTitle = "View collection";

    public const string Confirm = "/ui/collections/{collectionId}/confirm";

    public const string ConfirmTitle = "Confirm";

    public const string Cancel = "/ui/collections/{collectionId}/cancel";

    public const string CancelTitle = "Cancel";

    public const string Upload = "/ui/collections/{collectionId}/upload-document";

    public const string UploadTitle = "Upload";

    public static void RegisterCollectionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/collections")
        .WithTags("collections");

        group.MapPost("/{collectionId:guid}/confirm", ConfirmCollection.Handle);

        group.MapGet("/", ListCollections.Handle);

        group.MapGet("/{collectionId:guid}", GetCollection.Handle);

        var uigroup = app.MapGroup("/ui/collections")
           .ExcludeFromDescription()
           .RequireAuthorization();

        uigroup.MapGet("/list", ListCollections.HandlePage);

        uigroup.MapGet("/{collectionId:guid}/view", GetCollection.HandlePage);

        uigroup.MapGet("/{collectionId:guid}/confirm", ConfirmCollection.HandlePage);

        uigroup.MapPost("/{collectionId:guid}/confirm", ConfirmCollection.HandleAction);

        uigroup.MapPost("/{collectionId:guid}/cancel", CancelCollection.HandleAction);

        uigroup.MapGet("/{collectionId:guid}/upload-document", UploadDocument.HandlePage);

        uigroup.MapPost("/{collectionId:guid}/upload-document", UploadDocument.HandleAction)
            .DisableAntiforgery(); ;
    }
}