using Tests.Infrastructure;

namespace Tests.ProformaToCollaboratorPaymentProcesses;

public class StartProformaToCollaboratorPaymentProcessTests : BaseTest
{
    [Fact]
    public async Task start_should_be_ok()
    {
        var today = _appDsl.Clock.Now.DateTime;

        var (proformaResult, proformaCommand, clientResult, collaboratorResult) = await _appDsl.IssueProforma(_appDsl.Clock.Now.DateTime);

        await _appDsl.RegisterInvoice(proformaResult.ProformaId, clientResult.ClientId, proformaCommand.Currency);

        await _appDsl.ProformaToCollaboratorPaymentProcess.Start(c =>
        {
            c.CollaboratorId = collaboratorResult.CollaboratorId;
            c.Currency = proformaCommand.Currency;
            c.ProformaId = new[] { proformaResult.ProformaId };
        });
    }

    [Fact]
    public async Task start_should_throw_an_error_when_proforma_not_issue()
    {
        var (proformaResult, proformaCommand, _, collaboratorResult) = await _appDsl.RegisterProformaReadyToIssue(_appDsl.Clock.Now.DateTime);

        await _appDsl.ProformaToCollaboratorPaymentProcess.Start(c =>
        {
            c.CollaboratorId = collaboratorResult.CollaboratorId;
            c.Currency = proformaCommand.Currency;
            c.ProformaId = new[] { proformaResult.ProformaId };
        }, errorDetail: "code: proforma-is-issued");
    }
}