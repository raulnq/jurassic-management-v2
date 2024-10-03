namespace WebAPI.MoneyExchanges;

public static class Endpoints
{
    public const string Title = "Money Exchanges";

    public const string List = "/ui/money-exchanges/list";

    public const string ListTitle = "List money exchanges";

    public const string Register = "/ui/money-exchanges/register";

    public const string RegisterTitle = "Register money exchange";

    public const string Edit = "/ui/money-exchanges/{moneyExchangeId}/edit";

    public const string EditTitle = "Edit money exchange";

    public const string Upload = "/ui/money-exchanges/{moneyExchangeId}/upload-document";

    public const string UploadTitle = "Upload";

    public static void RegisterMoneyExchangeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/money-exchanges")
        .WithTags("money-exchanges");

        group.MapPost("/", RegisterMoneyExchange.Handle);

        group.MapGet("/", ListMoneyExchanges.Handle);

        group.MapGet("/{moneyExchangeId:guid}", GetMoneyExchange.Handle);

        group.MapPut("/{moneyExchangeId:guid}", EditMoneyExchange.Handle);

        var uigroup = app.MapGroup("/ui/money-exchanges")
        .ExcludeFromDescription()
        .RequireAuthorization();

        uigroup.MapGet("/list", ListMoneyExchanges.HandlePage);

        uigroup.MapGet("/register", RegisterMoneyExchange.HandlePage);

        uigroup.MapPost("/register", RegisterMoneyExchange.HandleAction);

        uigroup.MapGet("/{moneyExchangeId:guid}/edit", EditMoneyExchange.HandlePage);

        uigroup.MapPost("/{moneyExchangeId:guid}/edit", EditMoneyExchange.HandleAction);

        uigroup.MapGet("/{moneyExchangeId:guid}/upload-document", UploadDocument.HandlePage);

        uigroup.MapPost("/{moneyExchangeId:guid}/upload-document", UploadDocument.HandleAction)
            .DisableAntiforgery(); ;
    }
}
