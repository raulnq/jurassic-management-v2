using Tests.Infrastructure;

namespace Tests.Invoices;

public class CancelInvoiceTests : BaseTest
{
    [Fact]
    public async Task cancel_should_be_ok()
    {
        var today = _appDsl.Clock.Now.DateTime;

        var (proformaResult, proformaCommand, clientResult, _) = await _appDsl.IssueProforma(today);

        var start = await _appDsl.RegisterInvoice(proformaResult.ProformaId, clientResult.ClientId, proformaCommand.Currency);

        await _appDsl.Invoice.Cancel(start!.InvoiceId);

    }
}