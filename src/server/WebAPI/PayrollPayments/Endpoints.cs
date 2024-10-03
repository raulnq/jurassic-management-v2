namespace WebAPI.PayrollPayments;

public static class Endpoints
{
    public const string Title = "Payroll Payments";

    public const string List = "/ui/payroll-payments/list";

    public const string ListTitle = "List payroll payments";

    public const string Register = "/ui/payroll-payments/register";

    public const string RegisterTitle = "Register payroll payment";

    public const string Edit = "/ui/payroll-payments/{payrollPaymentId}/edit";

    public const string EditTitle = "Edit payroll payment";

    public const string Pay = "/ui/payroll-payments/{payrollPaymentId}/pay";

    public const string PayTitle = "Pay";

    public const string PayAfp = "/ui/payroll-payments/{payrollPaymentId}/pay-afp";

    public const string PayAfpTitle = "Pay AFP";

    public const string Upload = "/ui/payroll-payments/{payrollPaymentId}/upload-document";

    public const string UploadTitle = "Upload";

    public const string Cancel = "/ui/payroll-payments/{payrollPaymentId}/cancel";

    public const string ExcludeFromTaxes = "/ui/payroll-payments/{payrollPaymentId}/exclude";

    public const string ExcludeFromTaxesTitle = "Exclude";

    public const string IncludeFromTaxes = "/ui/payroll-payments/{payrollPaymentId}/include";

    public const string IncludeFromTaxesTitle = "Include";

    public const string CancelTitle = "Cancel";

    public static void RegisterPayrollPaymentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/payroll-payments")
        .WithTags("payroll-payments");

        group.MapPost("/{payrollPaymentId:guid}/upload-document", UploadDocument.Handle)
            .DisableAntiforgery();

        group.MapPost("/{payrollPaymentId:guid}/pay", PayPayrollPayment.Handle);

        group.MapPost("/{payrollPaymentId:guid}/pay-afp", PayAfpPayrollPayment.Handle);

        group.MapPost("/", RegisterPayrollPayment.Handle);

        group.MapPut("/{payrollPaymentId:guid}", EditPayrollPayment.Handle);

        group.MapGet("/", ListPayrollPayments.Handle);

        var uigroup = app.MapGroup("/ui/payroll-payments")
           .ExcludeFromDescription()
           .RequireAuthorization();

        uigroup.MapGet("/list", ListPayrollPayments.HandlePage);

        uigroup.MapGet("/{payrollPaymentId:guid}/edit", EditPayrollPayment.HandlePage);

        uigroup.MapPost("/{payrollPaymentId:guid}/edit", EditPayrollPayment.HandleAction);

        uigroup.MapGet("/register", RegisterPayrollPayment.HandlePage);

        uigroup.MapPost("/register", RegisterPayrollPayment.HandleAction);

        uigroup.MapGet("/{payrollPaymentId:guid}/upload-document", UploadDocument.HandlePage);

        uigroup.MapPost("/{payrollPaymentId:guid}/upload-document", UploadDocument.HandleAction)
            .DisableAntiforgery();

        uigroup.MapGet("/{payrollPaymentId:guid}/pay", PayPayrollPayment.HandlePage);

        uigroup.MapPost("/{payrollPaymentId:guid}/pay", PayPayrollPayment.HandleAction);

        uigroup.MapGet("/{payrollPaymentId:guid}/pay-afp", PayAfpPayrollPayment.HandlePage);

        uigroup.MapPost("/{payrollPaymentId:guid}/pay-afp", PayAfpPayrollPayment.HandleAction);

        uigroup.MapPost("/{payrollPaymentId:guid}/cancel", CancelPayrollPayment.HandleAction);

        uigroup.MapPost("/{payrollPaymentId:guid}/include", IncludePayrollPayment.HandleAction);

        uigroup.MapPost("/{payrollPaymentId:guid}/exclude", ExcludePayrollPayment.HandleAction);
    }
}
