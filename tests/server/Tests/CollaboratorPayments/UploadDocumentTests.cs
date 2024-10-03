using Tests.Infrastructure;

namespace Tests.CollaboratorPayments;

public class UploadDocumentTests : BaseTest
{
    [Fact]
    public async Task upload_should_be_ok()
    {
        var today = _appDsl.Clock.Now.DateTime;

        var (proformaResult, proformaCommand, clientResult, collaboratorResult) = await _appDsl.IssueProforma(today);

        await _appDsl.RegisterInvoice(proformaResult.ProformaId, clientResult.ClientId, proformaCommand.Currency);

        var start = await _appDsl.PayCollaboratorPayment(proformaResult.ProformaId, collaboratorResult.CollaboratorId, proformaCommand.Currency, today);

        await _appDsl.CollaboratorPayment.Upload("blank.pdf", start!.CollaboratorPaymentId);
    }
}