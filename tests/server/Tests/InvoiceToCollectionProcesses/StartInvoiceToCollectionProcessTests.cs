using Tests.Infrastructure;

namespace Tests.InvoiceToCollectionProcesses;

public class StartInvoiceToCollectionProcessTests : BaseTest
{
    [Fact]
    public async Task start_should_be_ok()
    {
        var today = _appDsl.Clock.Now.DateTime;

        var (proformaResult, proformaCommand, clientResult, _) = await _appDsl.IssueProforma(today);

        var invoice = await _appDsl.IssueInvoice(proformaResult.ProformaId, clientResult.ClientId, proformaCommand.Currency, today);

        await _appDsl.InvoiceToCollectionProcess.Start(c =>
        {
            c.ClientId = clientResult.ClientId;
            c.Currency = proformaCommand.Currency;
            c.InvoiceId = new[] { invoice.InvoiceId };
        });
    }

    [Fact]
    public async Task start_should_throw_an_error_when_invoice_not_issue()
    {
        var (proformaResult, proformaCommand, clientResult, _) = await _appDsl.IssueProforma(_appDsl.Clock.Now.DateTime);

        var (_, start) = await _appDsl.ProformaToInvoiceProcess.Start(c =>
        {
            c.Currency = proformaCommand.Currency;
            c.ClientId = clientResult.ClientId;
            c.ProformaId = new[] { proformaResult.ProformaId };
        });

        await _appDsl.InvoiceToCollectionProcess.Start(c =>
        {
            c.InvoiceId = new[] { start!.InvoiceId };
        }, errorDetail: "code: invoice-is-not-issued");
    }
}