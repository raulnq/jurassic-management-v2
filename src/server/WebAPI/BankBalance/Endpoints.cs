namespace WebAPI.BankBalance;

public static class Endpoints
{
    public const string Title = "Bank Balance";

    public const string ListTitle = "List bank balance";

    public const string List = "/ui/bank-balance/list";

    public static void RegisterBankBalanceEndpoints(this WebApplication app)
    {
        var uigroup = app.MapGroup("/ui/bank-balance")
        .ExcludeFromDescription()
        .RequireAuthorization();

        uigroup.MapGet("/list", ListBankBalance.HandlePage);
    }
}
