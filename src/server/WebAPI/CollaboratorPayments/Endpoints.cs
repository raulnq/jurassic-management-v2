namespace WebAPI.CollaboratorPayments;

public static class Endpoints
{
    public const string Title = "Collaborator Payments";

    public const string List = "/ui/collaborator-payments/list";

    public const string ListTitle = "List collaborator payments";

    public const string Register = "/ui/collaborator-payments/register";

    public const string RegisterTitle = "Register collaborator payment";

    public const string Edit = "/ui/collaborator-payments/{collaboratorPaymentId}/edit";

    public const string EditTitle = "Edit collaborator payment";

    public const string Pay = "/ui/collaborator-payments/{collaboratorPaymentId}/pay";

    public const string PayTitle = "Pay";

    public const string Confirm = "/ui/collaborator-payments/{collaboratorPaymentId}/confirm";

    public const string ConfirmTitle = "Confirm";

    public const string Upload = "/ui/collaborator-payments/{collaboratorPaymentId}/upload-document";

    public const string UploadTitle = "Upload";

    public const string Cancel = "/ui/collaborator-payments/{collaboratorPaymentId}/cancel";

    public const string CancelTitle = "Cancel";

    public const string Send = "/ui/collaborator-payments/{collaboratorPaymentId}/send";

    public static void RegisterCollaboratorPaymentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/collaborator-payments")
        .WithTags("collaborator-payments");

        group.MapPost("/{collaboratorPaymentId:guid}/upload-document", UploadDocument.Handle)
            .DisableAntiforgery();

        group.MapPost("/{collaboratorPaymentId:guid}/confirm", ConfirmCollaboratorPayment.Handle);

        group.MapPost("/{collaboratorPaymentId:guid}/pay", PayCollaboratorPayment.Handle);

        group.MapPost("/", RegisterCollaboratorPayment.Handle);

        group.MapPut("/{collaboratorPaymentId:guid}", EditCollaboratorPayment.Handle);

        group.MapGet("/", ListCollaboratorPayments.Handle);

        var uigroup = app.MapGroup("/ui/collaborator-payments")
           .ExcludeFromDescription()
           .RequireAuthorization();

        uigroup.MapGet("/list", ListCollaboratorPayments.HandlePage);

        uigroup.MapGet("/{collaboratorPaymentId:guid}/edit", EditCollaboratorPayment.HandlePage);

        uigroup.MapPost("/{collaboratorPaymentId:guid}/edit", EditCollaboratorPayment.HandleAction);

        uigroup.MapGet("/register", RegisterCollaboratorPayment.HandlePage);

        uigroup.MapPost("/register", RegisterCollaboratorPayment.HandleAction);

        uigroup.MapGet("/{collaboratorPaymentId:guid}/upload-document", UploadDocument.HandlePage);

        uigroup.MapPost("/{collaboratorPaymentId:guid}/upload-document", UploadDocument.HandleAction)
            .DisableAntiforgery();

        uigroup.MapGet("/{collaboratorPaymentId:guid}/pay", PayCollaboratorPayment.HandlePage);

        uigroup.MapPost("/{collaboratorPaymentId:guid}/pay", PayCollaboratorPayment.HandleAction);

        uigroup.MapGet("/{collaboratorPaymentId:guid}/confirm", ConfirmCollaboratorPayment.HandlePage);

        uigroup.MapPost("/{collaboratorPaymentId:guid}/confirm", ConfirmCollaboratorPayment.HandleAction);

        uigroup.MapPost("/{collaboratorPaymentId:guid}/cancel", CancelCollaboratorPayment.HandleAction);

        uigroup.MapPost("/{collaboratorPaymentId:guid}/send", SendCollaboratorPayment.HandleAction);
    }
}
