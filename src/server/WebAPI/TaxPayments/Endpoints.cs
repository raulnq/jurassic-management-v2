namespace WebAPI.TaxPayments;

public static class Endpoints
{
    public const string Title = "Tax Payments";

    public const string List = "/ui/tax-payments/list";

    public const string ListTitle = "List tax payments";

    public const string Register = "/ui/tax-payments/register";

    public const string RegisterTitle = "Register tax payments";

    public const string View = "/ui/tax-payments/{taxPaymentId}/view";

    public const string ViewTitle = "View tax payments";

    public const string Add = "/ui/tax-payments/{taxPaymentId}/taxes/add";

    public const string Load = "/ui/tax-payments/{taxPaymentId}/taxes/load";

    public const string Remove = "/ui/tax-payments/{taxPaymentId}/taxes/{taxPaymentItemId}/remove";

    public const string Edit = "/ui/tax-payments/{taxPaymentId}/taxes/{taxPaymentItemId}/edit";

    public const string AddTitle = "Add Tax";

    public const string EditTitle = "Edit Tax";

    public const string LoadTitle = "Load Taxes";

    public const string ItemTitle = "Taxes";

    public const string Pay = "/ui/tax-payments/{taxPaymentId}/pay";

    public const string PayTitle = "Pay";

    public const string Upload = "/ui/tax-payments/{taxPaymentId}/upload-document";

    public const string UploadTitle = "Upload";

    public static void RegisterTaxPaymentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/tax-payments")
        .WithTags("tax-payments");

        group.MapPost("/", RegisterTaxPayment.Handle);

        group.MapGet("/", ListTaxPayments.Handle);

        //group.MapGet("/{clientId:guid}", GetClient.Handle);

        //group.MapPut("/{clientId:guid}", EditClient.Handle);

        var uigroup = app.MapGroup("/ui/tax-payments")
        .ExcludeFromDescription()
        .RequireAuthorization();

        uigroup.MapGet("/list", ListTaxPayments.HandlePage);

        uigroup.MapGet("/register", RegisterTaxPayment.HandlePage);

        uigroup.MapPost("/register", RegisterTaxPayment.HandleAction);

        uigroup.MapGet("/{taxPaymentId:guid}/view", GetTaxPayment.HandlePage);

        uigroup.MapGet("/{taxPaymentId:guid}/taxes/add", AddTax.HandlePage);

        uigroup.MapPost("/{taxPaymentId:guid}/taxes/add", AddTax.HandleAction);

        uigroup.MapGet("/{taxPaymentId:guid}/pay", PayTaxPayment.HandlePage);

        uigroup.MapPost("/{taxPaymentId:guid}/pay", PayTaxPayment.HandleAction);

        uigroup.MapPost("/{taxPaymentId:guid}/taxes/load", LoadTaxes.HandleAction);

        uigroup.MapDelete("/{taxPaymentId:guid}/taxes/{taxPaymentItemId:int}/remove", RemoveTax.HandleAction);

        uigroup.MapGet("/{taxPaymentId:guid}/taxes/{taxPaymentItemId:int}/edit", EditTax.HandlePage);

        uigroup.MapPost("/{taxPaymentId:guid}/taxes/{taxPaymentItemId:int}/edit", EditTax.HandleAction);

        uigroup.MapGet("/{taxPaymentId:guid}/upload-document", UploadDocument.HandlePage);

        uigroup.MapPost("/{taxPaymentId:guid}/upload-document", UploadDocument.HandleAction)
            .DisableAntiforgery();
    }
}