using Tests.Infrastructure;

namespace Tests.ProformaToInvoiceProcesses;

public class StartProformaToInvoiceProcessTests : BaseTest
{
    [Fact]
    public async Task start_should_be_ok()
    {
        var (proformaResult, proformCommand, clientResult, _) = await _appDsl.IssueProforma(_appDsl.Clock.Now.DateTime);

        await _appDsl.ProformaToInvoiceProcess.Start(c =>
        {
            c.Currency = proformCommand.Currency;
            c.ClientId = clientResult.ClientId;
            c.ProformaId = new[] { proformaResult.ProformaId };
        });
    }

    [Fact]
    public async Task start_should_throw_an_error_when_proforma_not_issue()
    {
        var (proformaResult, proformCommand, clientResult, _) = await _appDsl.RegisterProformaReadyToIssue(_appDsl.Clock.Now.DateTime);

        await _appDsl.ProformaToInvoiceProcess.Start(c =>
        {
            c.Currency = proformCommand.Currency;
            c.ClientId = clientResult.ClientId;
            c.ProformaId = new[] { proformaResult.ProformaId };
        }, errorDetail: "code: proforma-is-not-issued");
    }
}