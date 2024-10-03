namespace WebAPI.ProformaToCollaboratorPaymentProcesses;

public static class Endpoints
{
    public const string RegisterTitle = "Register collaborator payment from proformas";

    public const string Register = "/ui/proforma-to-collaborator-payment-processes/register";

    public const string ListItems = "/ui/proforma-to-collaborator-payment-processes/{collaboratorPaymentId}/items/list";

    public static void RegisterProformaToColaboratorPaymentProcessEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/proforma-to-collaborator-payment-processes")
        .WithTags("proforma-to-collaborator-payment-processes");

        group.MapPost("/", StartProformaToCollaboratorPaymentProcess.Handle);

        var uigroup = app.MapGroup("/ui/proforma-to-collaborator-payment-processes")
        .ExcludeFromDescription()
        .RequireAuthorization();

        uigroup.MapGet("/register", StartProformaToCollaboratorPaymentProcess.HandlePage);

        uigroup.MapPost("/register", StartProformaToCollaboratorPaymentProcess.HandleAction);

        uigroup.MapGet("/{collaboratoPaymentId:guid}/items/list", ListProformaToCollaboratorPaymentProcessItems.HandlePage);

    }
}