namespace WebAPI.Transactions;

public static class Endpoints
{
    public const string Title = "Transactions";

    public const string List = "/ui/transactions/list";

    public const string ListTitle = "List transactions";

    public const string Register = "/ui/transactions/register";

    public const string RegisterTitle = "Register transaction";

    public const string Edit = "/ui/transactions/{transactionId}/edit";

    public const string EditTitle = "Edit transaction";

    public const string Upload = "/ui/transactions/{transactionId}/upload-document";

    public const string UploadTitle = "Upload";

    public static void RegisterTransactionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/transactions")
        .WithTags("transactions");

        group.MapPost("/", RegisterTransaction.Handle);

        group.MapGet("/", ListTransactions.Handle);

        group.MapGet("/{transactionId:guid}", GetTransaction.Handle);

        group.MapPut("/{transactionId:guid}", EditTransaction.Handle);

        var uigroup = app.MapGroup("/ui/transactions")
        .ExcludeFromDescription()
        .RequireAuthorization();

        uigroup.MapGet("/list", ListTransactions.HandlePage);

        uigroup.MapGet("/register", RegisterTransaction.HandlePage);

        uigroup.MapPost("/register", RegisterTransaction.HandleAction);

        uigroup.MapGet("/{transactionId:guid}/edit", EditTransaction.HandlePage);

        uigroup.MapPost("/{transactionId:guid}/edit", EditTransaction.HandleAction);

        uigroup.MapGet("/{transactionId:guid}/upload-document", UploadDocument.HandlePage);

        uigroup.MapPost("/{transactionId:guid}/upload-document", UploadDocument.HandleAction)
            .DisableAntiforgery(); ;
    }
}
