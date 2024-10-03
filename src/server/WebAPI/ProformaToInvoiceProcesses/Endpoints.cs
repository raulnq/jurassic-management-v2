namespace WebAPI.ProformaToInvoiceProcesses;

public static class Endpoints
{
    public const string RegisterTitle = "Register invoice";

    public const string Register = "/ui/proforma-to-invoice-processes/register";

    public const string ListItems = "/ui/proforma-to-invoice-processes/{invoiceId}/items/list";

    public static void RegisterProformaToInvoiceProcessEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/proforma-to-invoice-processes")
        .WithTags("proforma-to-invoice-processes");

        group.MapPost("/", StartProformaToInvoiceProcess.Handle);

        var uigroup = app.MapGroup("/ui/proforma-to-invoice-processes")
        .ExcludeFromDescription()
        .RequireAuthorization();

        uigroup.MapGet("/register", StartProformaToInvoiceProcess.HandlePage);

        uigroup.MapPost("/register", StartProformaToInvoiceProcess.HandleAction);

        uigroup.MapGet("/{invoiceId:guid}/items/list", ListProformaToInvoiceProcessItems.HandlePage);
    }
}
